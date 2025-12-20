# ReSys.Shop Admin Panel

This is the official admin panel for the ReSys.Shop e-commerce platform, built with Vue 3 and Vite. It provides a comprehensive interface for store administrators to manage products, orders, customers, and other core aspects of the shop.

## Features

- **Dashboard**: An overview of key store metrics and recent activity.
- **Product Management**: A complete interface to create, update, and manage products, variants, options, categories, and properties.
- **Order Management**: View and process customer orders.
- **Image Management**: Upload product images and manage visual assets.
- **Background Job Control**: Interface with the backend to trigger and monitor jobs, such as generating image embeddings via the Image Search microservice.
- **Security Dashboard**: Monitor and review potential infrastructure vulnerabilities reported by integrated security tools like Checkov.

## Architecture

The admin panel is a modern Single-Page Application (SPA) built on the following technologies:
- **Vue 3**: The core progressive JavaScript framework.
- **Vite**: For a fast development experience with hot-module replacement.
- **Pinia/Vuex**: For centralized state management (assumed, choose as appropriate).
- **ESLint & Prettier**: For code linting and consistent formatting.

It communicates with the main **`ReSys.Shop.Api` (.NET)** backend via RESTful or GraphQL APIs to fetch data and perform actions.

## Recommended IDE Setup

[VS Code](https://code.visualstudio.com/) + [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur if installed).

## Project Setup

### 1. Prerequisites
- [Node.js](https://nodejs.org/) (LTS version recommended)
- `npm`, `pnpm`, or `yarn`

### 2. Installation
Install the project dependencies:
```sh
npm install
```

### 3. Configuration
The admin panel needs to know the URL of the backend API. This is configured using environment variables.

1.  Create a new file named `.env.local` in the root of the `/src/ReSys.Shop.Admin` directory.
2.  Add the following environment variable to the file, pointing to your running .NET backend API:

    ```
    VITE_API_BASE_URL=https://localhost:7001
    ```
    *Replace the URL with the actual address of your `ReSys.Shop.Api` service.*

## Development Workflow

### Compile and Hot-Reload for Development
This command starts the local development server with hot-reloading enabled.
```sh
npm run dev
```

### Compile and Minify for Production
This command builds the application for production, creating optimized static assets in the `dist` directory.
```sh
npm run build
```

### Lint with [ESLint](https://eslint.org/)
This command runs the linter to check for code quality and style issues.
```sh
npm run lint
```