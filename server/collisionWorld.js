import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import * as THREE from "three";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const DATA_DIR = path.join(__dirname, "data");

const EDGE_SAMPLES = 8;

function pointXZ(pt) {
    if (Array.isArray(pt)) return [pt[0], pt[1]];
    return [pt.x, pt.z];
}

function pointInPolygon(x, z, points) {
    let inside = false;
    for (let i = 0, j = points.length - 1; i < points.length; j = i++) {
        const [xi, zi] = pointXZ(points[i]);
        const [xj, zj] = pointXZ(points[j]);
        const intersect =
            zi > z !== zj > z &&
            x < ((xj - xi) * (z - zi)) / (zj - zi) + xi;
        if (intersect) inside = !inside;
    }
    return inside;
}

function circleHitsBox(x, z, radius, box) {
    const halfW = box.w / 2;
    const halfD = box.d / 2;
    const closestX = Math.max(box.x - halfW, Math.min(x, box.x + halfW));
    const closestZ = Math.max(box.z - halfD, Math.min(z, box.z + halfD));
    const dx = x - closestX;
    const dz = z - closestZ;
    return dx * dx + dz * dz < radius * radius;
}

function closestPointOnSegment(px, pz, ax, az, bx, bz) {
    const abx = bx - ax;
    const abz = bz - az;
    const apx = px - ax;
    const apz = pz - az;
    const abLenSq = abx * abx + abz * abz;
    let t = abLenSq > 1e-8 ? (apx * abx + apz * abz) / abLenSq : 0;
    t = Math.max(0, Math.min(1, t));
    return [ax + abx * t, az + abz * t];
}

function zoneCentroid(points) {
    let cx = 0;
    let cz = 0;
    for (const pt of points) {
        const [x, z] = pointXZ(pt);
        cx += x;
        cz += z;
    }
    return [cx / points.length, cz / points.length];
}

export const collisionWorld = {
    scenes: {},

    loadAll() {
        this.scenes = {};
        if (!fs.existsSync(DATA_DIR)) {
            console.warn("collisionWorld: no data directory");
            return;
        }
        const files = fs.readdirSync(DATA_DIR).filter(f => f.startsWith("collision_") && f.endsWith(".json"));
        for (const file of files) {
            const raw = JSON.parse(fs.readFileSync(path.join(DATA_DIR, file), "utf8"));
            const sceneName = raw.scene ?? file.replace(/^collision_|\.json$/g, "");
            this.scenes[sceneName] = {
                zones: raw.zones ?? [],
                obstacles: raw.obstacles ?? [],
            };
            console.log(`collisionWorld: loaded ${file} (${this.scenes[sceneName].zones.length} zones, ${this.scenes[sceneName].obstacles.length} obstacles)`);
        }
    },

    getScene(sceneName) {
        return this.scenes[sceneName] ?? null;
    },

    isWalkable(x, z, sceneName) {
        const data = this.getScene(sceneName);
        if (!data || data.zones.length === 0) return true;
        for (const zone of data.zones) {
            if (zone.points?.length >= 3 && pointInPolygon(x, z, zone.points))
                return true;
        }
        return false;
    },

    hitsObstacle(x, z, radius, sceneName) {
        const data = this.getScene(sceneName);
        if (!data?.obstacles) return false;
        for (const box of data.obstacles) {
            if (circleHitsBox(x, z, radius, box)) return true;
        }
        return false;
    },

    canOccupy(x, z, radius, sceneName) {
        if (!this.isWalkable(x, z, sceneName)) return false;
        if (this.hitsObstacle(x, z, radius, sceneName)) return false;

        for (let i = 0; i < EDGE_SAMPLES; i++) {
            const a = (i / EDGE_SAMPLES) * Math.PI * 2;
            const px = x + Math.cos(a) * radius;
            const pz = z + Math.sin(a) * radius;
            if (!this.isWalkable(px, pz, sceneName)) return false;
            if (this.hitsObstacle(px, pz, 0.05, sceneName)) return false;
        }
        return true;
    },

    findNearestWalkable(x, z, radius, sceneName) {
        const data = this.getScene(sceneName);
        if (!data?.zones?.length) return { x, z };

        const candidates = [];

        for (const zone of data.zones) {
            const pts = zone.points;
            if (!pts || pts.length < 3) continue;

            const [centX, centZ] = zoneCentroid(pts);

            if (pointInPolygon(x, z, pts)) {
                candidates.push({ x, z, distSq: 0 });
            }

            for (let i = 0; i < pts.length; i++) {
                const j = (i + 1) % pts.length;
                const [xi, zi] = pointXZ(pts[i]);
                const [xj, zj] = pointXZ(pts[j]);
                const [cx, cz] = closestPointOnSegment(x, z, xi, zi, xj, zj);
                const dx = x - cx;
                const dz = z - cz;
                candidates.push({ x: cx, z: cz, distSq: dx * dx + dz * dz, centX, centZ });
            }

            for (const pt of pts) {
                const [px, pz] = pointXZ(pt);
                const dx = x - px;
                const dz = z - pz;
                candidates.push({ x: px, z: pz, distSq: dx * dx + dz * dz, centX, centZ });
            }
        }

        candidates.sort((a, b) => a.distSq - b.distSq);

        for (const c of candidates) {
            if (this.canOccupy(c.x, c.z, radius, sceneName))
                return { x: c.x, z: c.z };

            const centX = c.centX ?? c.x;
            const centZ = c.centZ ?? c.z;
            for (let step = 1; step <= 20; step++) {
                const t = step / 20;
                const ix = c.x + (centX - c.x) * t;
                const iz = c.z + (centZ - c.z) * t;
                if (this.canOccupy(ix, iz, radius, sceneName))
                    return { x: ix, z: iz };
            }
        }

        return { x, z };
    },

    resolveMove(from, to, radius, sceneName) {
        const fromVec =
            from instanceof THREE.Vector3
                ? from.clone()
                : new THREE.Vector3(from.x, from.y, from.z);
        const toVec =
            to instanceof THREE.Vector3
                ? to.clone()
                : new THREE.Vector3(to.x, to.y, to.z);

        if (!this.getScene(sceneName))
            return toVec;

        if (this.canOccupy(toVec.x, toVec.z, radius, sceneName))
            return toVec;

        let lo = 0;
        let hi = 1;
        for (let i = 0; i < 14; i++) {
            const mid = (lo + hi) / 2;
            const testX = fromVec.x + (toVec.x - fromVec.x) * mid;
            const testZ = fromVec.z + (toVec.z - fromVec.z) * mid;
            if (this.canOccupy(testX, testZ, radius, sceneName)) lo = mid;
            else hi = mid;
        }

        let resultX = fromVec.x + (toVec.x - fromVec.x) * lo;
        let resultZ = fromVec.z + (toVec.z - fromVec.z) * lo;
        let resultY = fromVec.y + (toVec.y - fromVec.y) * lo;

        if (!this.canOccupy(resultX, resultZ, radius, sceneName)) {
            const nearest = this.findNearestWalkable(
                toVec.x,
                toVec.z,
                radius,
                sceneName
            );
            resultX = nearest.x;
            resultZ = nearest.z;
            resultY = toVec.y;
        }

        return new THREE.Vector3(resultX, resultY, resultZ);
    },
};
