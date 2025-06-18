import apiClient from './apiClient';

const machineAPI = {
    getAll() {
        const url = `/MachineInfo/machines`;
        return apiClient.get(url);
    },

    getMachineById(id) {
        const url = `/machines/${id}`;
        return apiClient.get(url);
    },

    create(machine) {
        const url = '/machines';
        return apiClient.post(url, machine);
    },

    update(id, machine) {
        const url = `/machines/${id}`;
        return apiClient.put(url, machine);
    },

    delete(id) {
        const url = `/machines/${id}`;
        return apiClient.delete(url);
    },

    // Thêm các phương thức đặc biệt cho máy
    updateParameters(id, parameters) {
        const url = `/machines/${id}/parameters`;
        return apiClient.put(url, { parameters });
    },

    getMachineStatus(id) {
        const url = `/machines/${id}/status`;
        return apiClient.get(url);
    }
};

export default machineAPI; 