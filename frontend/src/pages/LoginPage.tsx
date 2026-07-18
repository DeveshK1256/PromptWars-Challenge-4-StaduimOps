// src/pages/LoginPage.tsx
import { useState } from "react";
import { signInWithGoogle, signInWithEmail, registerWithEmail, resetPassword } from "../firebase";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [mode, setMode] = useState<"login" | "register" | "reset">("login");
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const handleGoogle = async () => {
    try {
      setError(null);
      await signInWithGoogle();
    } catch (e: any) {
      setError(e.message);
    }
  };

  const handleEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      if (mode === "login") {
        await signInWithEmail(email, password);
      } else if (mode === "register") {
        await registerWithEmail(email, password);
        setSuccessMsg("Registration successful! Please verify your email.");
      } else {
        await resetPassword(email);
        setSuccessMsg("Password reset email sent!");
      }
    } catch (e: any) {
      setError(e.message);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="text-5xl mb-3">🏟️</div>
          <h1 className="text-3xl font-bold text-white">StadiumOps</h1>
          <p className="text-blue-300 mt-1">FIFA World Cup 2026 Smart Operations</p>
        </div>
        <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-2xl p-8 shadow-2xl">
          <h2 className="text-xl font-semibold text-white mb-6">
            {mode === "login" ? "Sign In" : mode === "register" ? "Create Account" : "Reset Password"}
          </h2>
          {error && (
            <div role="alert" aria-live="assertive" className="bg-red-500/20 border border-red-400 text-red-200 rounded-lg p-3 mb-4 text-sm">
              {error}
            </div>
          )}
          {successMsg && (
            <div role="status" aria-live="polite" className="bg-green-500/20 border border-green-400 text-green-200 rounded-lg p-3 mb-4 text-sm">
              {successMsg}
            </div>
          )}
          {mode === "login" && (
            <button
              onClick={handleGoogle}
              aria-label="Sign in with Google account"
              className="w-full flex items-center justify-center gap-3 py-3 mb-6 bg-white text-gray-800 font-medium rounded-xl hover:bg-gray-100 transition-all duration-200 shadow"
            >
              <svg className="w-5 h-5" viewBox="0 0 24 24" aria-hidden="true">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              Continue with Google
            </button>
          )}
          <form onSubmit={handleEmail} noValidate aria-label={mode === "login" ? "Email sign in form" : mode === "register" ? "Registration form" : "Password reset form"}>
            <div className="space-y-4">
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-blue-200 mb-1">Email address</label>
                <input
                  id="email"
                  type="email"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  className="w-full px-4 py-3 bg-white/10 border border-white/20 rounded-xl text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-transparent transition"
                  placeholder="you@example.com"
                  required
                  autoComplete="email"
                  aria-required="true"
                />
              </div>
              {mode !== "reset" && (
                <div>
                  <label htmlFor="password" className="block text-sm font-medium text-blue-200 mb-1">Password</label>
                  <input
                    id="password"
                    type="password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    className="w-full px-4 py-3 bg-white/10 border border-white/20 rounded-xl text-white placeholder-white/40 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-transparent transition"
                    placeholder="••••••••••"
                    required={mode !== "reset"}
                    autoComplete={mode === "login" ? "current-password" : "new-password"}
                    aria-required="true"
                    minLength={10}
                  />
                </div>
              )}
              <button
                type="submit"
                className="w-full py-3 bg-blue-600 hover:bg-blue-500 text-white font-semibold rounded-xl transition-all duration-200 shadow-lg hover:shadow-blue-500/25"
              >
                {mode === "login" ? "Sign In" : mode === "register" ? "Create Account" : "Send Reset Email"}
              </button>
            </div>
          </form>
          <div className="mt-6 flex justify-between text-sm">
            <button onClick={() => { setMode("register"); setError(null); }} className="text-blue-300 hover:text-white transition" type="button">Create account</button>
            <button onClick={() => { setMode("reset"); setError(null); }} className="text-blue-300 hover:text-white transition" type="button">Forgot password?</button>
            {mode !== "login" && <button onClick={() => { setMode("login"); setError(null); }} className="text-blue-300 hover:text-white transition" type="button">Back to login</button>}
          </div>
        </div>
      </div>
    </div>
  );
}
