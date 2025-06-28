import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextField, Button, Box, Typography } from '@mui/material';
import { loginUser } from '../api/authApi';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    const response = await loginUser(email, password);

    if (response.success) {
      // Save session info (for now, using localStorage)
      localStorage.setItem('token', response.token);
      localStorage.setItem('role', response.role);

      // Redirect based on role
      if (response.role === 'admin') {
        navigate('/admin/dashboard');
      } else {
        navigate('/tickets/open');
      }
    } else {
      setError(response.message);
    }
  };

  return (
    <Box sx={{ width: 300, margin: 'auto', marginTop: '20vh' }}>
      <Typography variant="h5" gutterBottom>Login</Typography>
      <TextField
        label="Email"
        variant="outlined"
        fullWidth
        margin="normal"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <TextField
        label="Password"
        type="password"
        variant="outlined"
        fullWidth
        margin="normal"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      {error && (
        <Typography color="error" variant="body2">{error}</Typography>
      )}
      <Button
        variant="contained"
        color="primary"
        fullWidth
        sx={{ mt: 2 }}
        onClick={handleLogin}
      >
        Login
      </Button>
    </Box>
  );
}

export default Login;
