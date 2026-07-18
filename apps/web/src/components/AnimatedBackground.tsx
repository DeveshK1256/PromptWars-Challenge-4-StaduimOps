import { Canvas, useFrame } from "@react-three/fiber";
import { useRef, useMemo } from "react";
import * as THREE from "three";

function FloatingParticles({ count = 120 }: { count?: number }) {
  const ref = useRef<THREE.Points>(null);

  const particles = useMemo(() => {
    const positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);
    const sizes = new Float32Array(count);

    for (let i = 0; i < count; i++) {
      positions[i * 3] = (Math.random() - 0.5) * 20;
      positions[i * 3 + 1] = (Math.random() - 0.5) * 14;
      positions[i * 3 + 2] = (Math.random() - 0.5) * 10;

      const palette = [
        [0.06, 0.46, 0.43], // teal
        [0.18, 0.83, 0.75], // cyan
        [0.72, 0.47, 0.12], // gold
        [0.37, 0.88, 0.72], // mint
      ];
      const c = palette[Math.floor(Math.random() * palette.length)];
      colors[i * 3] = c[0];
      colors[i * 3 + 1] = c[1];
      colors[i * 3 + 2] = c[2];

      sizes[i] = Math.random() * 0.08 + 0.02;
    }
    return { positions, colors, sizes };
  }, [count]);

  useFrame((state) => {
    if (!ref.current) return;
    const positions = ref.current.geometry.attributes.position.array as Float32Array;
    const time = state.clock.elapsedTime;

    for (let i = 0; i < count; i++) {
      positions[i * 3 + 1] += Math.sin(time * 0.3 + i * 0.5) * 0.002;
      positions[i * 3] += Math.cos(time * 0.2 + i * 0.3) * 0.001;
    }
    ref.current.geometry.attributes.position.needsUpdate = true;
    ref.current.rotation.y = time * 0.02;
  });

  return (
    <points ref={ref}>
      <bufferGeometry>
        <bufferAttribute attach="attributes-position" args={[particles.positions, 3]} />
        <bufferAttribute attach="attributes-color" args={[particles.colors, 3]} />
      </bufferGeometry>
      <pointsMaterial size={0.08} vertexColors transparent opacity={0.7} sizeAttenuation depthWrite={false} />
    </points>
  );
}

function GlowOrbs() {
  const group = useRef<THREE.Group>(null);

  useFrame((state) => {
    if (group.current) {
      group.current.rotation.y = state.clock.elapsedTime * 0.05;
    }
  });

  return (
    <group ref={group}>
      {[
        { pos: [-3, 2, -2] as [number, number, number], color: "#0f766e", scale: 0.8 },
        { pos: [4, -1, -3] as [number, number, number], color: "#2dd4bf", scale: 0.5 },
        { pos: [-2, -2, 1] as [number, number, number], color: "#b7791f", scale: 0.6 },
        { pos: [3, 3, -1] as [number, number, number], color: "#5eead4", scale: 0.4 },
      ].map((orb, i) => (
        <mesh key={i} position={orb.pos} scale={orb.scale}>
          <sphereGeometry args={[1, 32, 32]} />
          <meshStandardMaterial
            color={orb.color}
            emissive={orb.color}
            emissiveIntensity={0.6}
            transparent
            opacity={0.15}
            roughness={1}
          />
        </mesh>
      ))}
    </group>
  );
}

export function AnimatedBackground() {
  return (
    <div className="animated-bg">
      <Canvas camera={{ position: [0, 0, 6], fov: 60 }} dpr={[1, 1.5]}>
        <ambientLight intensity={0.2} />
        <FloatingParticles count={150} />
        <GlowOrbs />
        <fog attach="fog" args={["#030712", 5, 15]} />
      </Canvas>
    </div>
  );
}
