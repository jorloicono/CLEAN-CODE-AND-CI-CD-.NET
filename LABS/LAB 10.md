## How to Create a "Hello World" Workflow in Github Actions

To create a workflow in Github Actions that prints "Hello World" every time a push is made to the repository, follow these steps:

1. Open your Github repository and navigate to the **"Actions"** tab.

2. Click on **"New Workflow"** and select **"Set up a workflow yourself"**.

3. In the text editor that appears, paste the following YAML code:

```yaml
name: Hola Mundo Workflow
on:
  push:
    branches:
      - main
jobs:
  hola-mundo:
    runs-on: ubuntu-latest
    steps:
      - name: Print greeting
        run: echo "Hola Mundo!"

```

4. The code above specifies that the workflow will be activated on every push to the **"main"** branch of the repository.

5. The job **"hola-mundo"** will print **"Hola Mundo!"** in the console.

6. Click on **"Start commit"** and then on **"Commit new file"** to save the changes to the repository.

---

### **Testing the Workflow**

7. You should now see your new workflow in the **"Actions"** tab.

8. Make a push to the **"main"** branch of your repository and you should see the workflow activate and print **"Hola Mundo!"** in the console.

---

### **Customization**

That's it! You now have a workflow in Github Actions that will print **"Hola Mundo!"** every time a push is made to the **"main"** branch of the repository. You can customize this workflow to perform other actions and specify which branches will activate the workflow.
