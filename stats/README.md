<!-- STATS_BADGES_START -->
![Classes](https://img.shields.io/badge/Classes-204-blue?style=flat-square)
![Lines of Code](https://img.shields.io/badge/Lines_of_Code-34682-green?style=flat-square)
![C# Files](https://img.shields.io/badge/C%23_Files-182-orange?style=flat-square)
![TSCN Files](https://img.shields.io/badge/TSCN_Files-48-purple?style=flat-square)
![TRES Files](https://img.shields.io/badge/TRES_Files-16-red?style=flat-square)
![Last Updated](https://img.shields.io/badge/Updated-2025--08--07-lightgrey?style=flat-square)
<!-- STATS_BADGES_END -->

<!-- STATS_TABLE_START -->
| Statistics | Count | Description |
|------------|-------|--------------|
| C# Files | **182** | Number of C# script files in the project |
| Classes | **204** | Total number of defined classes |
| Lines of Code | **34682** | Total lines of code (excluding empty lines and comments) |
| TSCN Files | **48** | Number of Godot scene files |
| TRES Files | **16** | Number of Godot resource files |

> Last updated: 2025-08-07 06:20:20
<!-- STATS_TABLE_END -->

# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default tseslint.config({
  extends: [
    // Remove ...tseslint.configs.recommended and replace with this
    ...tseslint.configs.recommendedTypeChecked,
    // Alternatively, use this for stricter rules
    ...tseslint.configs.strictTypeChecked,
    // Optionally, add this for stylistic rules
    ...tseslint.configs.stylisticTypeChecked,
  ],
  languageOptions: {
    // other options...
    parserOptions: {
      project: ['./tsconfig.node.json', './tsconfig.app.json'],
      tsconfigRootDir: import.meta.dirname,
    },
  },
})
```

You can also install [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default tseslint.config({
  extends: [
    // other configs...
    // Enable lint rules for React
    reactX.configs['recommended-typescript'],
    // Enable lint rules for React DOM
    reactDom.configs.recommended,
  ],
  languageOptions: {
    // other options...
    parserOptions: {
      project: ['./tsconfig.node.json', './tsconfig.app.json'],
      tsconfigRootDir: import.meta.dirname,
    },
  },
})
```
