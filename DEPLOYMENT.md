# GitHub Pages Deployment

This repository is configured to automatically deploy the React frontend to GitHub Pages using GitHub Actions.

## Setup

### 1. Enable GitHub Pages

1. Go to your repository settings on GitHub
2. Navigate to **Settings** → **Pages**
3. Under **Source**, select **GitHub Actions**
4. Save the configuration

### 2. Deployment

The deployment happens automatically when:
- You push changes to the `main` branch that affect the `frontend/` directory
- You manually trigger the workflow from the Actions tab

### 3. Accessing the Site

Once deployed, your site will be available at:
```
https://bsnooks.github.io/FantasyArchive/
```

## Workflow Details

The GitHub Actions workflow (`.github/workflows/deploy.yml`) performs these steps:

1. **Build**: 
   - Checks out the code
   - Sets up Node.js 18
   - Installs dependencies with `npm ci`
   - Builds the React app with `npm run build`
   - Sets the correct `PUBLIC_URL` for GitHub Pages subdirectory

2. **Deploy**:
   - Uploads the build artifacts
   - Deploys to GitHub Pages using the official GitHub Pages action

## Local Development vs Production

- **Local**: Runs on `http://localhost:3000/`
- **GitHub Pages**: Runs on `https://bsnooks.github.io/FantasyArchive/`

The app automatically detects the environment and adjusts the routing accordingly.

## Manual Deployment

To manually trigger a deployment:

1. Go to the **Actions** tab in your GitHub repository
2. Select the "Deploy React App to GitHub Pages" workflow
3. Click **Run workflow** → **Run workflow**

## Troubleshooting

### Common Issues

1. **404 on page refresh**: GitHub Pages and React Router need special handling for client-side routing
2. **Assets not loading**: Ensure the `homepage` field in `package.json` matches your repository name
3. **Build failures**: Check the Actions tab for detailed error logs

### Build Requirements

- Node.js 18+
- All dependencies must be properly declared in `package.json`
- Build must complete successfully with `npm run build`

## Data Files

Note: The frontend expects data files to be available in the `public/data/` directory. In a production setup, you may want to:

1. Set up a separate data API
2. Use a CDN for static data files
3. Include data generation in the build process

For now, ensure your data files are properly placed in `frontend/public/data/` before deployment.