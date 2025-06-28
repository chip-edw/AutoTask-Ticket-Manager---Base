# Tailwind CSS Usage in ATTMS

## ✅ Version in Use

We are using:

```
tailwindcss@3.4.3
```

## 📌 Why We Chose v3.4.3 for the MVP

Tailwind v4 introduced a new PostCSS plugin system (`@tailwindcss/postcss`) which does not integrate cleanly with Vite + React + JSX in our current setup. During testing, Tailwind v4 caused:

- Utility classes like `bg-yellow-300`, `text-white`, and `border-red-600` to be omitted from compiled output
- `@tailwind` directives in `index.css` to be passed through unprocessed
- False confidence due to partial utility generation (e.g., `p-4` would work but not color or text classes)

Despite having correct `content` paths and safelisting, v4 failed to compile a full utility set through Vite.

---

## ✅ v3.4.3 Works Without Issues

Tailwind v3.4.3 provides:

- Full compatibility with Vite and JSX
- JIT compilation with no configuration workarounds
- Stable PostCSS setup using the standard plugin

### Working Setup

**`postcss.config.cjs`**
```js
module.exports = {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
};
```

**`index.css`**
```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

**Install with:**
```bash
npm install -D tailwindcss@3.4.3 postcss autoprefixer
```

---

## 📅 Revisit Tailwind v4 When...

We’ll consider upgrading after MVP launch when:

- The Tailwind v4 PostCSS plugin has clearer Vite/React integration
- Full utility generation is verified inside JSX
- No regressions occur from switching plugin syntax

---

## Developer Notes

- No hidden `div` safelist hacks are required in v3
- Utility-first approach is encouraged for all filters and table styling
- `FiltersBar.jsx` and `TicketTable.jsx` are clean and style-aware

---
