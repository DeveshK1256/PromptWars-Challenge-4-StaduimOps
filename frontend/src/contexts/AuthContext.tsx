import { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { onAuthStateChangedListener } from "../firebase";

interface User {
  uid: string;
  email: string | null;
  displayName: string | null;
}

interface AuthContextValue {
  user: User | null;
  loading: boolean;
  accessToken: string | null;
  setAccessToken: (token: string | null) => void;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  loading: true,
  accessToken: null,
  setAccessToken: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [accessToken, setAccessToken] = useState<string | null>(
    () => localStorage.getItem("access_token")
  );

  useEffect(() => {
    const unsubscribe = onAuthStateChangedListener((firebaseUser: any) => {
      setUser(firebaseUser ? { uid: firebaseUser.uid, email: firebaseUser.email, displayName: firebaseUser.displayName } : null);
      setLoading(false);
    });
    return unsubscribe;
  }, []);

  const handleSetToken = (token: string | null) => {
    setAccessToken(token);
    if (token) localStorage.setItem("access_token", token);
    else localStorage.removeItem("access_token");
  };

  return (
    <AuthContext.Provider value={{ user, loading, accessToken, setAccessToken: handleSetToken }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
