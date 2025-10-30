import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5000/api', // 后端 API 地址
  timeout: 5000,
});

export function login(username, password) {
  return apiClient.post('/auth/login', { username, password });
}
