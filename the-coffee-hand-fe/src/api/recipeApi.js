import apiClient from './apiClient';

export const recipeAPI = {
    async getAll() {
        try {
            const url = `/MachineInfo/recipes`;
            const response = await apiClient.get(url);
            console.log('API Response in recipeAPI:', response);
            return response;
        } catch (error) {
            console.error('Error in recipeAPI.getAll:', error);
            throw error;
        }
    },

    async getById(id) {
        try {
            const url = `/MachineInfo/recipes/${id}`;
            const response = await apiClient.get(url);
            console.log('API Response in recipeAPI.getById:', response);
            return response;
        } catch (error) {
            console.error('Error in recipeAPI.getById:', error);
            throw error;
        }
    },

    async createRecipe(recipeData) {
        try {
            const url = `/MachineInfo/recipes`;
            const response = await apiClient.post(url, recipeData);
            return response;
        } catch (error) {
            console.error('Error in recipeAPI.createRecipe:', error);
            throw error;
        }
    },

    async createRecipeWithVolume(volumeData) {
        try {
            const url = `/recipes`;
            const response = await apiClient.post(url, volumeData);
            return response;
        } catch (error) {
            console.error('Error in recipeAPI.createRecipeWithVolume:', error);
            throw error;
        }
    },

    async createRecipeStep(drinkId, recipeData) {
        try {
            const url = `/MachineInfo/create/${drinkId}`;
            const response = await apiClient.post(url, recipeData);
            return response;
        } catch (error) {
            console.error('Error in recipeAPI.createRecipeStep:', error);
            throw error;
        }
    }
};

export default recipeAPI; 