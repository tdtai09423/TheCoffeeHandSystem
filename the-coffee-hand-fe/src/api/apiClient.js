import axios from "axios";
import config from "./config";  // Import từ file config.js
import authAPI from "./authApi";

const apiClient = axios.create({
    baseURL: process.env.NEXT_PUBLIC_API_BASE_URL || config.API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    }
});

// Add a request interceptor
apiClient.interceptors.request.use(function (config) {
    const token = authAPI.getToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, function (error) {
    // Do something with request error
    return Promise.reject(error);
});

// Add a response interceptor
apiClient.interceptors.response.use(function (response) {
    // Any status code that lie within the range of 2xx cause this function to trigger
    // Do something with response data
    return response.data;
}, function (error) {
    // Any status codes that falls outside the range of 2xx cause this function to trigger
    // Do something with response error
    if (error.response?.status === 401) {
        // Handle unauthorized access
        authAPI.logout();
    }
    return Promise.reject(error);
});

export default apiClient;
