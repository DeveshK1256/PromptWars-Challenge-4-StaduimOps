import { Canvas, useFrame } from "@react-three/fiber";
import { OrbitControls, Float, Text } from "@react-three/drei";
import { useRef, useMemo } from "react";
import * as THREE from "three";

function StadiumRing({
  radius,
  height,
  yOffset,
  color,
  density,
}: {
  radius: number;
  height: number;
  yOffset: number;
  color: string;
  density: number;
}) {
  const meshRef = useRef<THREE.Mesh>(null);
  const emissiveIntensity = density > 80 ? 0.8 : density > 60 ? 0.4 : 0.15;

  useFrame((_, delta) => {
    if (meshRef.current) {
      meshRef.current.rotation.y += delta * 0.05;
    }
  });

  return (
    <mesh ref={meshRef} position={[0, yOffset, 0]}>
      <torusGeometry args={[radius, height, 16, 64]} />
      <meshStandardMaterial
        color={color}
        emissive={color}
        emissiveIntensity={emissiveIntensity}
        transparent
        opacity={0.85}
        roughness={0.3}
        metalness={0.6}
      />
    </mesh>
  );
}

function StadiumField() {
  return (
    <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.1, 0]}>
      <circleGeometry args={[1.8, 64]} />
      <meshStandardMaterial color="#15803d" emissive="#15803d" emissiveIntensity={0.2} roughness={0.8} />
    </mesh>
  );
}

function CrowdParticles({ count = 200, density = 50 }: { count?: number; density?: number }) {
  const points = useMemo(() => {
    const positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const r = 2.2 + Math.random() * 1.5;
      positions[i * 3] = Math.cos(angle) * r;
      positions[i * 3 + 1] = Math.random() * 1.5 - 0.3;
      positions[i * 3 + 2] = Math.sin(angle) * r;

      const hue = density > 70 ? 0.0 : density > 50 ? 0.12 : 0.35;
      const color = new THREE.Color().setHSL(hue, 0.9, 0.6);
      colors[i * 3] = color.r;
      colors[i * 3 + 1] = color.g;
      colors[i * 3 + 2] = color.b;
    }
    return { positions, colors };
  }, [count, density]);

  const ref = useRef<THREE.Points>(null);

  useFrame((_, delta) => {
    if (ref.current) {
      ref.current.rotation.y += delta * 0.08;
    }
  });

  return (
    <points ref={ref}>
      <bufferGeometry>
        <bufferAttribute attach="attributes-position" args={[points.positions, 3]} />
        <bufferAttribute attach="attributes-color" args={[points.colors, 3]} />
      </bufferGeometry>
      <pointsMaterial size={0.06} vertexColors transparent opacity={0.8} sizeAttenuation />
    </points>
  );
}

function StadiumLabel({ text, position }: { text: string; position: [number, number, number] }) {
  return (
    <Float speed={2} floatIntensity={0.3}>
      <Text position={position} fontSize={0.18} color="#ffffff" anchorX="center" anchorY="middle" font={undefined}>
        {text}
      </Text>
    </Float>
  );
}

type ZoneData = {
  name: string;
  density: number;
  status: string;
};

export function Stadium3D({
  zones = [],
  stadiumName = "Stadium",
}: {
  zones?: ZoneData[];
  stadiumName?: string;
}) {
  const tiers = zones.length > 0 ? zones : defaultZones;

  return (
    <div className="stadium-3d-container">
      <Canvas camera={{ position: [5, 3, 5], fov: 45 }} dpr={[1, 2]}>
        <ambientLight intensity={0.4} />
        <directionalLight position={[5, 8, 5]} intensity={1.2} castShadow />
        <pointLight position={[-3, 4, -3]} intensity={0.6} color="#2dd4bf" />
        <pointLight position={[3, 4, 3]} intensity={0.4} color="#fbbf24" />

        <StadiumField />

        {tiers.map((zone, i) => {
          const color = zone.density > 80 ? "#ef4444" : zone.density > 60 ? "#f59e0b" : "#0f766e";
          return (
            <StadiumRing
              key={zone.name}
              radius={2.5 + i * 0.6}
              height={0.15 + i * 0.03}
              yOffset={i * 0.4}
              color={color}
              density={zone.density}
            />
          );
        })}

        <CrowdParticles count={300} density={tiers[0]?.density ?? 50} />

        <StadiumLabel text={stadiumName} position={[0, 2.2, 0]} />

        <OrbitControls
          enablePan={false}
          enableZoom={true}
          minDistance={3}
          maxDistance={12}
          autoRotate
          autoRotateSpeed={0.5}
          maxPolarAngle={Math.PI / 2.2}
        />

        <fog attach="fog" args={["#030712", 8, 18]} />
      </Canvas>
    </div>
  );
}

const defaultZones: ZoneData[] = [
  { name: "South Gates", density: 71, status: "Congested" },
  { name: "North Concourse", density: 58, status: "Elevated" },
  { name: "East Food Hall", density: 82, status: "Congested" },
  { name: "West Premium", density: 35, status: "Normal" },
];
