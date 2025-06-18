import axios from "./apiClient";

const orderAPI = {
    getAll: (pageNumber = 1, pageSize = 10, userId = "", dateStart = "", dateEnd = "") => {
        let url = `/orders/paginated?`;
        
        // Add optional filters if they exist
        if (userId) url += `userId=${userId}&`;
        if (dateStart) url += `dateStart=${dateStart}&`;
        if (dateEnd) url += `dateEnd=${dateEnd}&`;
        if (pageNumber) url += `pageNumber=${pageNumber}&`;
        if (pageSize) url += `pageSize=${pageSize}`;
        
        return axios.get(url);
    },
    getOrdersById: (id) => {
        const url = `/orders/${id}`;
        return axios.get(url);
    },
    create: (data) => {
        const url = '/orders';
        return axios.post(url, data);
    },
    update: (id, data) => {
        const url = `/orders/${id}`;
        return axios.put(url, data);
    },
    delete: (id) => {
        const url = `/orders/${id}`;
        return axios.delete(url);
    }
}

export default orderAPI;