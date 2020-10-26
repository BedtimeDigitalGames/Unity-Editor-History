# UPM Extensions

**UPM Extensions** integrates into UPM to allow for publishing packages and modifying the package manifest!

  - Increment Patch/Minor/Major versions of a package
  - Publish package to a private registry
  - Update the package manifest with that version of the package

## Requirements
  - Unity 2019.3+
  - Node.JS **(Only needed for publishing)*

## Installation in a new project
First, add the *Bedtime package registry* to the package manifest so it looks like this:
```
{
  "dependencies": {
    ...
  },
  "scopedRegistries": [
    {
      "name": "Bedtime",
      "url": "https://packages.bedtime.io",
      "scopes": [
        "com.bedtime"
      ]
    }
  ]
}
```
Furthermore, you'll want to add the **UPM Extensions** to the top of the manifest dependencies like so:
```
{
  "dependencies": {
    "com.bedtime.upm-extensions": "1.2.0",
    ...
  },
  "scopedRegistries": [
    {
      "name": "Bedtime",
      "url": "https://packages.bedtime.io",
      "scopes": [
        "com.bedtime"
      ]
    }
  ]
}
```

After this is done, you should have the **UPM Extensions** package in your project!

## Manual login
At this point, the only available package will be the **UPM Extensions** package itself, but now you can open the **preferences window** in *Unity*, and add the *registry* and *token* into it. This will unlock the rest of the *Bedtime* packages!

### Login through file
Alternatively, if you have or place a file named **upmlogin.json** in the root of yout *Unity* project, you can add the *registry* and *token* to that file, and have the **UPM Extensions** set up your data automatically! This is nice for sharing your project with others.

The **upmlogin.json** file should be formatted something like this, substitute the token and registry with your team's data:
```
{
  "registry": "https://packages.bedtime.io",
  "token": "xxxxxxxxxxxxxxxxxxxxxxxxxx"
}
```

## Publishing packages
Inside the *Unity Package Manager* window, you should now have some new options in the bottom toolbar for packages that you have locally in your project's *Packages* folder:

  - [0.0.1+] `Will increment the selected local package one patch version`
  - [0.1.0+] `Will increment the selected local package one minor version`
  - [1.0.0+] `Will increment the selected local package one major version`
  - [Publish] `Publish the package to your registry`
  - [Update Manifest] `Add or update the package in your manifest`
  - [Develop Mode] `Toggles the selected package between a local modifiable version, and a read-only online version`

A manifest update is also run automatically every time you publish, so this is generally only needed when you have local packages that are not in that project's manifest.
For a package to be able to go into develop mode, the **package.json** file must contain a *URL* in the *author* section of the file, like this:
```
  "author": {
    "name": "Bedtime Digital Games",
    "url": "https://git.bedtime.io/unity-packages/com.bedtime.upm-extensions"
  }
```

## Creating Packages
In the top right corner of the *Unity Package Manager* you can click the **New Package** button. This will allow you to create a package without setting up all the normal boilerplate files needed for a package.

## Workflow
Generally you'll want to increment your package version before commiting to version control. Publishing should be done when you actually want to give other people the new version of the package. If you commit your package changes first, you'll have an uncommited change in the **package.json** file of the package which you'll need to commit/amend as well.

#### A flow like this is recommended:
  - If package is not in project, add it to the project with the **install** button
  - Switch package to **Develop Mode**
  - Do your changes
  - **Increment** the package *version* with a Patch/Minor/Major version
  - **Commit** your changes to *git*
  - **Publish** to package registry
