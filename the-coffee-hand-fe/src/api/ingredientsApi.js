import apiClient from './apiClient';

const ingredientsAPI = {
    getAll(pageNumber = 1, pageSize = 10) {
        const url = `/ingredient/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        return apiClient.get(url);
    },

    create(ingredient) {
        const url = '/ingredient';
        return apiClient.post(url, ingredient);
    },

    update(ingredient) {
        const url = `/ingredient/${ingredient.id}`;
        return apiClient.put(url, ingredient);
    },

    delete(id) {
        const url = `/ingredient/${id}`;
        return apiClient.delete(url);
    },

    getIngredientById(id) {
        const url = `/ingredient/${id}`;
        return apiClient.get(url);
    },

    getIngredientByName(name) {
        const url = `/ingredient/by-name?name=${encodeURIComponent(name)}`;
        return apiClient.get(url);
    }
}

export default ingredientsAPI;