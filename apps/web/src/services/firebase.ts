import { initializeApp } from "firebase/app";
import {
  getAuth,
  signInWithEmailAndPassword,
  createUserWithEmailAndPassword,
  signInWithPopup,
  GoogleAuthProvider,
  sendEmailVerification,
  sendPasswordResetEmail,
  signOut,
  type User,
} from "firebase/auth";

// Firebase configuration — set these environment variables in .env
const firebaseConfig = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY || "",
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN || "",
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID || "",
  storageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET || "",
  messagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID || "",
  appId: import.meta.env.VITE_FIREBASE_APP_ID || "",
};

const isConfigured = Boolean(firebaseConfig.apiKey && firebaseConfig.projectId);

let app: ReturnType<typeof initializeApp> | null = null;
let auth: ReturnType<typeof getAuth> | null = null;

if (isConfigured) {
  app = initializeApp(firebaseConfig);
  auth = getAuth(app);
}

const googleProvider = new GoogleAuthProvider();
googleProvider.addScope("email");
googleProvider.addScope("profile");

export { isConfigured as firebaseEnabled };

export async function firebaseSignInWithGoogle() {
  if (!auth) throw new Error("Firebase not configured");
  const result = await signInWithPopup(auth, googleProvider);
  return mapFirebaseUser(result.user);
}

export async function firebaseSignInWithEmail(email: string, password: string) {
  if (!auth) throw new Error("Firebase not configured");
  const result = await signInWithEmailAndPassword(auth, email, password);
  return mapFirebaseUser(result.user);
}

export async function firebaseRegisterWithEmail(email: string, password: string) {
  if (!auth) throw new Error("Firebase not configured");
  const result = await createUserWithEmailAndPassword(auth, email, password);
  await sendEmailVerification(result.user);
  return mapFirebaseUser(result.user);
}

export async function firebaseResetPassword(email: string) {
  if (!auth) throw new Error("Firebase not configured");
  await sendPasswordResetEmail(auth, email);
}

export async function firebaseSignOut() {
  if (!auth) return;
  await signOut(auth);
}

export async function getFirebaseIdToken(): Promise<string | null> {
  if (!auth?.currentUser) return null;
  return auth.currentUser.getIdToken();
}

export type FirebaseUserInfo = {
  uid: string;
  email: string;
  displayName: string;
  photoURL: string | null;
  emailVerified: boolean;
  idToken: string;
};

async function mapFirebaseUser(user: User): Promise<FirebaseUserInfo> {
  const idToken = await user.getIdToken();
  return {
    uid: user.uid,
    email: user.email ?? "",
    displayName: user.displayName ?? user.email?.split("@")[0] ?? "User",
    photoURL: user.photoURL,
    emailVerified: user.emailVerified,
    idToken,
  };
}
