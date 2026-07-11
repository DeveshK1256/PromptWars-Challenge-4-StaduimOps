// src/pages/LoginPage.tsx
import { useState } from "react";
import { signInWithGoogle, signInWithEmail, registerWithEmail, resetPassword } from "../firebase";
import { useAuth } from "../auth/AuthProvider";

export default function LoginPage() {
  const { user, loading, setUser } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);

  const handleGoogle = async () => {
    try {
      const result = await signInWithGoogle();
      setUser(result.user);
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await signInWithEmail(email, password);
      setUser(result.user);
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleRegister = async () => {
    try {
      const result = await registerWithEmail(email, password);
      setUser(result.user);
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleReset = async () => {
    try {
      await resetPassword(email);
      alert("Password reset email sent");
    } catch (e: any) {
      setError(e.message);
    }
  };

  if (loading) return <div className="flex justify-center items-center h-screen">Loading...</div>;
  if (user) return <div className="flex justify-center items-center h-screen">You are already logged in.</div>;

  return (
    <div className="max-w-md mx-auto mt-10 p-6 bg-surface dark:bg-surface-dark rounded shadow">
      <h1 className="text-2xl font-bold mb-4">Login</h1>
      {error && <div className="text-red-600 mb-2">{error}</div>}
      <button
        onClick={handleGoogle}
        className="w-full py-2 mb-4 bg-primary text-white rounded hover:bg-primary-dark"
      >
        Sign in with Google
      </button>
      <form onSubmit={handleEmail} className="space-y-4">
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          className="w-full p-2 border rounded"
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          className="w-full p-2 border rounded"
          required
        />
        <button type="submit" className="w-full py-2 bg-secondary text-white rounded hover:bg-secondary-dark">
          Sign in
        </button>
      </form>
      <div className="mt-4 flex justify-between">
        <button onClick={handleRegister} className="text-sm underline">Register</button>
        <button onClick={handleReset} className="text-sm underline">Forgot password?</button>
      </div>
    </div>
  );
}
