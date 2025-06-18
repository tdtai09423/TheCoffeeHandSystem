import apiClient from './apiClient';

const usersAPI = {
    getAll(pageNumber = 1, pageSize = 10) {
        const url = `/users/search?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        return apiClient.get(url);
    },
    getUsersById(id) {
        const url = `/users/${id}`;
        return apiClient.get(url);
    },

}

export default usersAPI;