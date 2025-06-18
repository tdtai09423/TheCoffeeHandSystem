import apiClient from './apiClient';

const drinksAPI = {
    getAll(pageNumber = 1, pageSize = 10) {
        const url = `/drink/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        return apiClient.get(url);
    },
    getDrinksById(id) {
        const url = `/drink/${id}`;
        return apiClient.get(url);
    },
    addNewDrinks(data) {
        const url = '/drink';
        return apiClient.post(url, data);
    },
    deleteDrinksById(id) {
        const url = `/drink/${id}`;
        return apiClient.delete(url);
    },
    updateDrink(id, data) {
        const url = `/drink/${id}`;
        return apiClient.put(url, data);
    },
    uploadImage(file) {
        const url = '/image/upload';
        const formData = new FormData();
        formData.append('file', file);
        return apiClient.post(url, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
    },
}

export default drinksAPI;