// tailwind.config.cjs
module.exports = {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  darkMode: "class", // enable dark mode via class
  theme: {
    extend: {
      colors: {
        primary: "hsl(210, 90%, 55%)",
        secondary: "hsl(340, 80%, 55%)",
        accent: "hsl(45, 95%, 55%)",
        background: "hsl(0, 0%, 100%)",
        surface: "hsl(0, 0%, 98%)",
        "background-dark": "hsl(210, 10%, 12%)",
        "surface-dark": "hsl(210, 10%, 16%)",
      },
    },
  },
  plugins: [],
};
