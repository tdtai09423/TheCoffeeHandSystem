import apiClient from './apiClient';

const authAPI = {
    login(data) {
        const url = `/auth/email-login`;
        return apiClient.post(url, data, {
            headers: {
                "Content-Type": "application/json",
            },
        });
    },
    
    setToken(token) {
        localStorage.setItem('jwt_token', token);
    },
    
    getToken() {
        return localStorage.getItem('jwt_token');
    },
    
    removeToken() {
        localStorage.removeItem('jwt_token');
    },
    
    isAuthenticated() {
        return !!this.getToken();
    },
    
    logout() {
        this.removeToken();
        window.location.href = '/login';
    }
};

export default authAPI;
