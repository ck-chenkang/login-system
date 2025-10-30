<template>
  <div class="login-page">
    <div class="login-card">
      <el-form :model="form" ref="loginForm" label-position="left" label-width="64px" class="login-form">
        <h2 class="title">登录</h2>
        <el-form-item label="用户名" prop="username">
          <el-input v-model="form.username" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item label="密码" prop="password">
          <el-input v-model="form.password" type="password" placeholder="请输入密码" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" class="full" @click="handleLogin">登录</el-button>
        </el-form-item>
        <el-form-item>
          <el-button class="full" @click="goRegister">注册</el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
  
</template>

<script>
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '../store/user';
import { login } from '../api/auth';

export default {
  setup() {
    const router = useRouter();
    const userStore = useUserStore();

    const form = reactive({
      username: '',
      password: '',
    });

    const handleLogin = async () => {
      if (!form.username || !form.password) {
        return alert('用户名或密码不能为空');
      }
      try {
        const res = await login(form.username, form.password);
        userStore.setToken(res.data.token);
        router.push('/home');
      } catch (err) {
        console.error(err);
        alert('登录失败：' + (err.response?.data?.message || err.message));
      }
    };

    const goRegister = () => {
      const hasRegister = router.getRoutes().some(r => r.path === '/register');
      if (hasRegister) {
        router.push('/register');
      } else {
        alert('注册页面尚未配置');
      }
    };

    return { form, handleLogin, goRegister };
  },
};
</script>

<style scoped>
:root {
  --green-ink: #103a31;
  --green-ink-2: #0b2c25;
  --accent: #1f6f5b;
  --grid: rgba(255, 255, 255, 0.06);
}

.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background-color: var(--green-ink-2);
  background-image:
    linear-gradient(180deg, rgba(0,0,0,0.25), rgba(0,0,0,0.25)),
    url('/bg-industrial-green.svg');
  background-size: cover;
  background-position: center;
}

.login-card {
  width: min(92vw, 420px);
  padding: 28px 24px 22px;
  border-radius: 12px;
  border: 1px solid rgba(255,255,255,0.08);
  background: rgba(16, 58, 49, 0.72);
  box-shadow: 0 10px 30px rgba(0,0,0,0.35), inset 0 1px 0 rgba(255,255,255,0.06);
  backdrop-filter: blur(6px);
}

.login-form :deep(.el-form-item__label) {
  color: #e6fff5;
}

.login-form :deep(.el-input__wrapper) {
  background: rgba(0,0,0,0.25);
  box-shadow: 0 0 0 1px rgba(255,255,255,0.07) inset;
}

.login-form :deep(.el-input__inner) {
  color: #f3fff9;
}

.title {
  margin: 0 0 18px;
  text-align: center;
  color: #eafff7;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.full {
  width: 100%;
}

.full :deep(.el-button) {
  width: 100%;
}

.login-form :deep(.el-button--primary) {
  background-color: var(--accent);
  border-color: var(--accent);
}

.login-form :deep(.el-button--primary:hover) {
  filter: brightness(1.08);
}
</style>
