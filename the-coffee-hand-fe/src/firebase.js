// src/firebase.js
import { initializeApp } from "firebase/app";
import { getAuth, GoogleAuthProvider } from "firebase/auth";

// Cấu hình Firebase của bạn
const firebaseConfig = {
  apiKey: "AIzaSyBMw9ha3W4Cd5uTYoJ0BEE2ieKZeo5Liak",
  authDomain: "thecoffeehand-3bf49.firebaseapp.com",
  projectId: "thecoffeehand-3bf49",
  storageBucket: "thecoffeehand-3bf49.firebasestorage.app",
  messagingSenderId: "137349428578",
  appId: "1:137349428578:web:b327b29b43d2c5df5b283b",
  measurementId: "G-MDN5GXTQ3H"
};

// Khởi tạo Firebase app
const app = initializeApp(firebaseConfig);

// Lấy đối tượng authentication và tạo Google provider
const auth = getAuth(app);
const googleProvider = new GoogleAuthProvider();

export { auth, googleProvider };
