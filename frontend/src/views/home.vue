<template>
  <div>
    <h1>欢迎，{{ username }}</h1>
    <el-button type="danger" @click="logout">退出登录</el-button>
  </div>
</template>

<script>
import { useUserStore } from '../store/user';
import { useRouter } from 'vue-router';

export default {
  setup() {
    const router = useRouter();
    const userStore = useUserStore();
    const username = userStore.username || '用户';

    const logout = () => {
      userStore.logout();
      router.push('/login');
    };

    return { username, logout };
  },
};
</script>
