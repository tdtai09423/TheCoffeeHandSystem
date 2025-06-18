import apiClient from './apiClient';

const categoriesAPI = {
    getAll(pageNumber = 1, pageSize = 10) {
        const url = `/categories/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        return apiClient.get(url);
    },
    getCategoriesById(id) {
        const url = `/categories/${id}`;
        return apiClient.get(url);
    },
    addNewCategories(name) {
        const url = `/categories`;
        return apiClient.post(url, {
           "name": name
        }, {
            headers: {
                "Content-Type": "application/json",
            },
        });
    },
    deleteCategoriesById(id) {
        const url = `/categories/${id}`;
        return apiClient.delete(url);
    },
}

export default categoriesAPI;