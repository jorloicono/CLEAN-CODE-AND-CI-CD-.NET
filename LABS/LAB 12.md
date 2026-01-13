## Workflow Contexts

In GitHub Actions, a **context** is a set of predefined objects or variables that contain relevant information about the environment, events, or other data associated with the execution of a workflow. You can use contexts to access information about steps, workflow runs, jobs, and execution environments.

Whenever you want to access a context from a workflow file, you must use a syntax similar to .

For example:

```yaml
name: Simple Contexts Example
on: push
jobs:
  print-info:
    runs-on: ubuntu-latest
    steps:
      - name: Set custom environment variable
        run: echo "CUSTOM_VARIABLE=Hello, World!" >> $GITHUB_ENV
      - name: Print commit author and custom environment variable
        run: |
          echo "Commit author: ${{ github.actor }}"
          echo "Custom variable: ${{ env.CUSTOM_VARIABLE }}"

```

In this example, the `github.actor` context provides the username of the person who triggered the `push` event, and the `env.CUSTOM_VARIABLE` context refers to the custom environment variable set in the previous step. The workflow prints both values to the console.

Contexts can be used primarily anywhere in your workflow file. They are often used with expressions to check specific conditions. The following example uses the `if` statement to validate the `github` context. In this case, the job will only run if the result of the expression is approved:

`if: github.event_name == 'pull_request_review' && github.event.review.state == 'approved'`

The syntax for accessing a context is simple: you can use the index syntax `github['event_name']` or the property de-reference syntax `github.event_name`.

GitHub Actions currently supports the following contexts:

| Context Name | Type | Description |
| --- | --- | --- |
| **env** | object | Contains variables set in a workflow, job, or step. For more information, see env context. |
| **vars** | object | Contains variables set at the repository, organization, or environment levels. For more information, see vars context. |
| **job** | object | Information about the currently running job. For more information, see job context. |
| **jobs** | object | For reusable workflows only, contains outputs of jobs from the reusable workflow. For more information, see jobs context. |
| **steps** | object | Information about the steps that have been run in the current job. For more information, see steps context. |
| **runner** | object | Information about the runner that is running the current job. For more information, see runner context. |
| **secrets** | object | Contains the names and values of secrets that are available to a workflow run. For more information, see secrets context. |
| **strategy** | object | Information about the matrix execution strategy for the current job. For more information, see strategy context. |
| **matrix** | object | Contains the matrix properties defined in the workflow that apply to the current job. For more information, see matrix context. |
| **needs** | object | Contains the outputs of all jobs that are defined as a dependency of the current job. For more information, see needs context. |
| **inputs** | object | Contains the inputs of a reusable or manually triggered workflow. For more information, see inputs context. |

---

## Expressions

In GitHub Actions, **expressions** can be used to set variables in a workflow file and access contexts. An expression can also use a combination of literals, functions, contexts, and operators.

### Literal Expressions

Literals are represented by data types such as the following:

* **boolean**: `true` or `false`, case-insensitive.
* **null**.
* **number**: any number format that is JSON compatible.
* **string**: Single quotes must be used with strings.

For example:

```yaml
env:
  myNull: ${{ null }}
  myBoolean: ${{ false }}
  myIntegerNumber: ${{ 711 }}
  myFloatNumber: ${{ -9.2 }}
  myHexNumber: ${{ 0xff }}
  myExponentialNumber: ${{ -2.99e-2 }}
  myString: Mona the Octocat
  myStringInBraces: ${{ 'It\'s open source!' }}

```

### Operators

Operators are used within expressions to perform various operations, such as arithmetic, comparison, or logical operations. Expressions are wrapped in double curly braces . The supported operators in GitHub Actions workflow expressions are:

| Operator | Description |
| --- | --- |
| **()** | Logical grouping |
| **[ ]** | Index |
| **.** | Property de-reference |
| **!** | Not |
| **<** | Less than |
| **<=** | Less than or equal |
| **>** | Greater than |
| **>=** | Greater than or equal |
| **==** | Equal |
| **!=** | Not equal |
| **&&** | And |
| ** |  |

**Example workflow file:**

```yaml
name: Less Than or Equal Operator Example
on: push
jobs:
  check_number:
    runs-on: ubuntu-latest
    steps:
      - name: Check if number is less than or equal to limit
        run: |
          result=$((7<= 19? "true": "false"))
          echo "Is 7 less than or equal to 19? Result: ${{ 7 <= 19 }}"
          echo "Is 7 less than or equal to 19? Result (using shell): $result"

```

---

### Functions

**Functions** are predefined or built-in methods that can be used within expressions to perform various operations or manipulate data. Functions can be used inside the double curly braces  that denote an expression.

* **`starts with`** and **`ends with`**: `starts with('string')`, `ends with('string')`.
* **`toJSON`**: Returns a pretty-printed JSON representation of the value passed. An example is `toJSON(value)`.
* **`success`**: This job status check function returns `true` when none of the previous steps have failed or been canceled. An example is `if: ${{ success() }}`.
* **`always`**: This job status check function returns `true` even when canceled. An example is `if: ${{ always() }}`.
* **`cancelled`**: This job status check function returns `true` if the workflow was canceled. An example is `if: ${{ cancelled() }}`.
* **`failure`**: This job status check function returns `true` when any previous step in a job fails. An example is `if: ${{ failure() }}`.

For example:

```yaml
name: build
on: push
jobs:
  job1:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.set-matrix.outputs.matrix }}
    steps:
      - id: set-matrix
        run: echo
          "matrix={\"include\":[{\"project\":\"foo\",\"config\":\"Debug\"},{\"project\":\"bar\",\"config\":\"Release\"}]}" >> $GITHUB_OUTPUT
  job2:
    needs: job1
    runs-on: ubuntu-latest
    strategy:
      matrix: ${{ fromJSON(needs.job1.outputs.matrix) }}
    steps:
      - run: build

```

The preceding workflow sets a JSON matrix in one job and passes it to the next job using an output file and `fromJSON`.

---
