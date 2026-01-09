## GitHub Actions Environment Variables and Secrets Management Tutorial

In this tutorial, you will learn different ways to save your GitHub Actions environment variables and secrets that you can use when needed while working with GitHub Actions.

### Setting up GitHub Actions Environment Variables

When automating processes with the GitHub Actions workflow, you may need to attach environment variables to your workflows. You first need to create and specify custom environment variables in the workflow using the **ENV** keyword.

1. Create a directory named `.github/workflows` where you will store your workflow file.
2. Next, create a file with your preferred name in the `.github/workflows` directory. For this example, the file is called `main.yml`. Copy and paste the following code into the `main.yml` file. The code below sets and displays the `API_KEY` environment variable.

```yaml
name: env_tutorial
## Triggers the workflow on when there is a push, or
## pull request on the main branch
on: [pull_request, push]

env:
## Sets environment variable
  API_KEY: XXXXXXXXXXXX

jobs:
  job1:
## The type of runner that the job will run on,
## here it runs on ubuntu latest
    runs-on: ubuntu-latest
    steps:
      - name: step 1
## Reference your environment variables
        run: echo "The API key is: ${{env.API_KEY}}"

  job2:
    runs-on: ubuntu-latest
    steps:
      - name: step 1
## Another way reference your environment
## variables
        run: echo "The API key is: $API_KEY"

```

3. Now, push to the repository.
4. Next, open your web browser and navigate to your project on GitHub. Click on the **Actions** tab, then click on your current commit. You will see something like the image below, showing that GitHub has run the workflow.
5. Finally, click on `job1` or `job2`, and you will see that you have successfully referenced the environment variable you initialized.

### Defining an Environment Variable for a Job

Now that you have initialized the environment variable across the entire workflow file, any job can reference the environment variable. But perhaps you only want one job to reference the environment variable. If so, place the **env** keyword in the job itself.

1. Replace the code in your `main.yml` file with the code below. The following code shows that when you place the environment variable in a particular job, other jobs cannot reference the environment variable.

```yaml
name: env_tutorial
## Triggers the workflow on when there is a push, or
## pull request on the main branch
on: [pull_request, push]

jobs:
  job1:
## The type of runner that the job will run on
    runs-on: ubuntu-latest
    env:
## Environment variable
      API_KEY: XXXXXXXXXXXX
    steps:
      - name: step 1
## Reference your environment variables
        run: echo "The API key is: ${{env.API_KEY}}"

  job2:
    runs-on: ubuntu-latest
    steps:
      - name: step 1
## Another way reference your environment
## variables
        run: echo "The API key is: $API_KEY"

```

2. Commit your changes and push the code to GitHub Actions environment variables as you did in the previous section.
3. Finally, navigate to your project on GitHub, then click on `job1` and `job2` to see their comparison:
* **job1**: you will see that you have referenced the environment variable perfectly.
* **job2**: the API key is blank.



### Defining an Environment Variable for a Step

Now that you have learned how to specify environment variables within a job, you may wonder how you can do the same for steps. For the steps in a job, specify the environment variable within the step as you did for the job.

1. Replace the code in your `main.yml` file with the code below. In the following code, you specify the environment variable in `step 1` but not in `step 2`, and you will see the effect in the next steps.

```yaml
name: env_tutorial
## Triggers the workflow on when there is a push, or
## pull request on the main branch
on: [pull_request, push]

jobs:
  job1:
## The type of runner that the job will run on
    runs-on: ubuntu-latest
    steps:
      - name: step 1
        env:
## Environment variable for step 1
          API_KEY: XXXXXXXXXXXX
## Reference your environment variables
        run: echo "The API key is: ${{env.API_KEY}}"

      - name: step 2
## Reference your environment variables
        run: echo "The API key is: ${{env.API_KEY}}"

```

2. Now commit the changes and push the code to GitHub.
3. Finally, navigate to your project in GitHub Actions environment variables and click on `job1`. Although you reference both API keys in the same job (`job1`) in both steps, `step 2` failed to evaluate the API key (blank), as shown below. This is because you did not specify the environment variable inside `step 2` of your code.

### Managing Environment Variables via GitHub Actions Secrets

Instead of hardcoding, you might want to securely store your environment variable, and GitHub Secrets can do just that. GitHub Actions Secrets encrypt the values you place in the secrets, so they are not visible or readable in plain sight. The secret created with this method is accessible to the entire workflow, jobs, and steps; there are no restrictions.

To store your environment variable in GitHub Secrets:

1. First, push your code to GitHub as you did in the previous sections.
2. Next, navigate to your project on GitHub and click on the **Settings** tab. Click on **Secrets** in the tab below to start adding a secret.
3. Next, click on **New repository secret**, and you will see a form to fill out the details about the secret you are adding.
4. Fill out the form correctly (**Name** and **Value**) and click the **Add secret** button to submit. Now `API_KEY` is saved in GitHub Secrets. This way, GitHub securely sets environment variables as secrets that you can reference when working in GitHub Actions.
5. Edit your `main.yml` file and replace the `env` keyword with `secrets`. Below, you can see that you reference the API key in the format `${{secrets.API_KEY}}` instead of hardcoding the API key in the code.

```yaml
name: env_tutorial
## Triggers the workflow on when there is a push, or
## pull request on the main branch
on: [pull_request, push]

jobs:
  job1:
## The type of runner that the job will run on
    runs-on: ubuntu-latest
    steps:
      - name: step 1
## Reference your environment variables
        run: echo "The API key is: ${{secrets.API_KEY}}"

  job2:
    runs-on: ubuntu-latest
    steps:
      - name: step 1
## Reference your environment variables
        run: echo "The API key is: ${{secrets.API_KEY}}"

```

6. Finally, commit and push the code to GitHub and navigate to your project in GitHub Actions environment variables. You will see something like the image below, but you cannot see the actual `API_key` because GitHub encrypts the values you enter in secrets.

### Referencing GitHub Default Environment Variables

There are a couple of default environment variables provided by GitHub, which you can use to access file systems in the repository instead of hardcoded paths. GitHub default environment variables allow you to be more dynamic when referencing the environment variables provided to you by GitHub.

Some of the paths you can get with the default environment variables are the following:

* `GITHUB_JOB` – Provides the `job_id` of the current job.
* `GITHUB_ACTION` – Provides the `id` of the current action.
* `GITHUB_ACTION_PATH` – Provides the path where your action is located.
* `GITHUB_ACTOR` – Provides the name of the person or application that initiated the workflow, such as your GitHub username.
* `GITHUB_RUN_ID` – Provides the unique number of the `run` command.

Replace what you have in your `main.yml` file with the following code. The following code displays the default environment variable indicated in the code.

```yaml
name: env_tutorial
## Triggers the workflow on when there is a push or
## pull request on the main branch
on: [pull_request, push]

jobs:
  job1:
## The type of runner that the job will run on
    runs-on: ubuntu-latest
    steps:
      - name: step 1
        run: |
          echo "The job_id is: $GITHUB_JOB"  # reference the default environment variables
          echo "The id of this action is: $GITHUB_ACTION"  # reference the default environment variables
          echo "The run id is: $GITHUB_RUN_ID"
          echo "The GitHub Actor's username is: $GITHUB_ACTOR"

      - name: step 2
        run: |
          echo "The run id is: $GITHUB_RUN_ID"

```

Commit and push the code changes to GitHub, check your actions in your GitHub Actions environment variables project, and you will see something like the image below.

## Conclusion

Throughout this lab, you have learned how to manage GitHub Actions environment variables. You should now have a basic understanding of how to securely store environment variables and how to use the default ones provided by GitHub.
