/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        canvas: '#fafaf9',
        ink: {
          900: '#0f172a',
          800: '#1e293b',
          700: '#334155',
          600: '#475569',
          500: '#64748b',
          400: '#94a3b8',
        },
        academic: {
          900: '#1e1b4b',
          800: '#312e81',
          700: '#3730a3',
          600: '#4338ca',
          500: '#6366f1',
          50: '#eef2ff'
        },
        status: {
          available: '#059669',
          held: '#d97706',
          confirmed: '#2563eb',
          danger: '#e11d48'
        }
      },
      fontFamily: {
        sans: ['"Inter"', 'system-ui', '-apple-system', 'sans-serif'],
      }
    },
  },
  plugins: [],
}

