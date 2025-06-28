/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  safelist: [
    'bg-yellow-300',
    'border-red-600',
    'text-black',
    'text-white',
    'bg-emerald-400',
    'bg-gray-50',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
