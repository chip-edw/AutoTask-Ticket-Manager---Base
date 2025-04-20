export async function loginUser(email, password) {
  // Simulated login
  if (email === 'admin@example.com' && password === 'admin123') {
    return {
      success: true,
      token: 'fake-jwt-token-admin',
      role: 'admin',
    };
  } else if (email === 'user@example.com' && password === 'user123') {
    return {
      success: true,
      token: 'fake-jwt-token-user',
      role: 'user',
    };
  } else {
    return {
      success: false,
      message: 'Invalid email or password',
    };
  }
}
