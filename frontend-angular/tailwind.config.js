/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
        display: ['Orbitron', 'sans-serif'], // Added for futuristic headings
      },
      colors: {
        // Space Theme Colors
        'space-light': '#1A1A2E', // Soft Midnight Blue (Light Mode Bg)
        'space-dark': '#0B0C10',  // Dark Space Black (Dark Mode Bg)
        'space-secondary': '#2C3E50', // Space Gray
        'sky-blue': {
          DEFAULT: '#3498DB',
          glow: '#5DADE2', // Lighter for glow
        },
        'electric-purple': {
          DEFAULT: '#8E44AD',
          glow: '#A569BD', // Lighter for glow
        },
        'metallic': '#BDC3C7', // Silver
        'platinum': '#D5D8DC', // Platinum

        // Mapping to standard utilities
        'bg-primary': '#1A1A2E',
        'bg-secondary': '#0B0C10',

        'primary': {
          DEFAULT: '#3498DB', // Sky Blue
          hover: '#2980B9',
        },
        'accent': {
          DEFAULT: '#8E44AD', // Electric Purple
          hover: '#9B59B6',
        }
      },
      boxShadow: {
        'glow-blue': '0 0 10px rgba(52, 152, 219, 0.5)',
        'glow-purple': '0 0 10px rgba(142, 68, 173, 0.5)',
        'glow-metallic': '0 0 8px rgba(189, 195, 199, 0.3)',
      }
    },
  },
  plugins: [],
}
