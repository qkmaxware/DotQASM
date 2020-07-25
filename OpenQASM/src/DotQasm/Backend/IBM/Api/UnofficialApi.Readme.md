# Unofficial IBM Q-Experience REST API
**Updated: Jan 2020**
This unofficial API is an attempt to document the IBM Quantum Experience REST API for application developers looking to interface with IBM's quantum machines without using QISkit. The host for this REST API exists at `https://api.quantum-computing.ibm.com/api`. 

Given that this API is unofficial, it may not be up to date with the real API. Additionally, information that could not be determined via sleuthing is indicated with a `?` to denote that the purpose or value of something is unknown. 

- [Unofficial IBM Q-Experience REST API](#unofficial-ibm-q-experience-rest-api)
- [Endpoints](#endpoints)
  - [/backends](#backends)
    - [GET /backends/{device_name}/properties](#get-backendsdevice_nameproperties)
      - [Purpose](#purpose)
      - [Request Body](#request-body)
      - [Response Body](#response-body)
    - [GET /backend/{device_name}/defaults](#get-backenddevice_namedefaults)
      - [Purpose](#purpose-1)
      - [Request Body](#request-body-1)
      - [Response Body](#response-body-1)
    - [/backend/{device_name}/queue/status](#backenddevice_namequeuestatus)
      - [Purpose](#purpose-2)
      - [Request Body](#request-body-2)
      - [Response Body](#response-body-2)
  - [/jobs](#jobs)
    - [GET /jobs](#get-jobs)
      - [Purpose](#purpose-3)
      - [Request Body](#request-body-3)
      - [Response Body](#response-body-3)
    - [POST /jobs](#post-jobs)
      - [Purpose](#purpose-4)
      - [Request Body](#request-body-4)
      - [Response Body](#response-body-4)
    - [GET /jobs/{job_id}](#get-jobsjob_id)
      - [Purpose](#purpose-5)
      - [Request Body](#request-body-5)
      - [Response Body](#response-body-5)
    - [/jobs/{job_id}/cancel](#jobsjob_idcancel)
      - [Request Body](#request-body-6)
      - [Response Body](#response-body-6)
    - [/jobs/{job_id}/status](#jobsjob_idstatus)
      - [Request Body](#request-body-7)
      - [Response Body](#response-body-7)
    - [/jobs/{job_id}/properties](#jobsjob_idproperties)
    - [/jobs/{job_id}/jobDataUploaded](#jobsjob_idjobdatauploaded)
    - [/jobs/{job_id}/resultDownloaded](#jobsjob_idresultdownloaded)
    - [/job/{job_id}s/jobDownloadUrl](#jobjob_idsjobdownloadurl)
    - [/jobs/{job_id}/resultDownloadUrl](#jobsjob_idresultdownloadurl)
    - [/jobs/{job_id}/jobUploadUrl](#jobsjob_idjobuploadurl)
  - [/network](#network)
    - [GET /network](#get-network)
      - [Purpose](#purpose-6)
      - [Request Body](#request-body-8)
      - [Response Body](#response-body-8)
  - [/users](#users)
    - [POST /users/login](#post-userslogin)
      - [Purpose](#purpose-7)
      - [Request Body](#request-body-9)
      - [Response Body](#response-body-9)
    - [POST /users/loginWithToken](#post-usersloginwithtoken)
      - [Purpose](#purpose-8)
      - [Request Body](#request-body-10)
      - [Response Body](#response-body-10)
    - [GET /users/me](#get-usersme)
      - [Purpose](#purpose-9)
      - [Request Body](#request-body-11)
      - [Response Body](#response-body-11)

# Endpoints
## /backends
### GET /backends/{device_name}/properties
#### Purpose
Obtain the properties of a given quantum device.
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
{
    backend_name: "string",             // Device name
    last_update_date: "string",         // Device update timestamp
    backend_version: "string",          // Device version
    gates: [                            // Supported gates
        {
            gate: "string",
            name: "string",
            parametres: [
                {
                    date: "string",
                    name: "string",
                    unit: "string",
                    value: float
                }
            ],
            qubits: [int]
        }
        ...
    ],
    general: [],            
    qubits: []                          // Qubit mapping array
}
```
### GET /backend/{device_name}/defaults
#### Purpose
Get device pulse defaults.
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
{}
```
### /backend/{device_name}/queue/status
#### Purpose
Used to obtain the status of a quantum computer as well as the number of items waiting in its queue to be executed.
#### Request Body
```js
```
#### Response Body
```js
{
    state: bool,
    message: "string",
    status: "string",
    lengthQueue: int,
    backend_version: "string"
}
```

## /jobs
### GET /jobs
#### Purpose
Get all user submitted jobs.
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
[
    {
        qasms: [
            {
                qasm: "string",
                status: "string",
                executionId: "string",
                result: ?
            }
            ...
        ],
        kind: "string",
        shots: int,
        backend: {
            id: "string",
            name: "string"
        },
        status: "string",
        creationDate: Date,
        objectStorageInfo: {
            jobId: "string",
            uploadQObjectUrlEndpoint: "string",
            downloadQObjectUrlEndpoint: "string",
            jobQasmConverted: "string",
            validatedUrl: "string",
            validationUploadUrlEndpoint: "string",
        },
        summaryData: {
            size: {
                input: int,
                output: int
            },
            success: bool,
            summary: {
                max_qubits_used: int,
                gates_executed: int,
                qobj_config: {
                    n_qubits: int,
                    max_credits: int,
                    memory_slots: int,
                    memory: bool,
                    shots: int,
                    type: "string"
                },
                num_circuits: int,
                partial_validation: bool
            },
            resultTime: float
        }
        timePerStep: {
            CREATING: Date,
            ...
        },
        ip: {
            ip: "string",
            city: "string",
            country: "string",
            continent: "string"
        },
        hubInfo: {
            hub: {
                name: "string",
                priority: int
            },
            group: {
                name: "string",
                priority: int
            },
            project: {
                name: "string",
                priority: int
            }
        },
        codeId: "string",
        endDate: Date,
        cost: float,
        id: "string",
        userId: "string"
    }
    ...
]
```

### POST /jobs
#### Purpose
Submit a new job to a particular IBM quantum computer. To create the job, we submit a quantum object or `qObject` whose specification can be read about in the IBM article [Qiskit Backend Specifications for OpenQASM and OpenPulse Experiments](https://arxiv.org/abs/1809.03452).
#### Request Body
```js
{
    name: "string",
    qObject: {
        qobj_id: "string",
        type: "string",
        schema_version: "string",
        experiements: [
            {
                header: {},
                config: {},
                instructions: [
                    {
                        name: "string",
                        qubits: int[],
                        memory: int[],
                        register: int[]
                    }
                    ...
                ]
            }
            ...
        ],
        header: {},
        config: {
            shots: int,
            memory_slots: int
        }
    }
    backend: {
        name: "string"
    },
    shots: int,
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
{
    qasms: [
        {
            status: "string",
            executionId: "string"
        }
        ...
    ],
    qObject: {},
    kind: "string",
    shots: int,
    backend: {
        id: "string",
        name: "string"
    },
    status: "string",
    creationDate: Date,
    hubInfo: {},
    cost: float,
    id: "string",
    userId: "string",
    name: "string",
    timePerStep: {}
}
```

### GET /jobs/{job_id}
#### Purpose
Get the information on a specific job if you know the job's unique id
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
[
    {
        qasms: [
            {
                qasm: "string",
                status: "string",
                executionId: "string",
                result: ?
            }
            ...
        ],
        kind: "string",
        shots: int,
        backend: {
            id: "string",
            name: "string"
        },
        status: "string",
        creationDate: Date,
        objectStorageInfo: {
            jobId: "string",
            uploadQObjectUrlEndpoint: "string",
            downloadQObjectUrlEndpoint: "string",
            jobQasmConverted: "string",
            validatedUrl: "string",
            validationUploadUrlEndpoint: "string",
        },
        summaryData: {
            size: {
                input: int,
                output: int
            },
            success: bool,
            summary: {
                max_qubits_used: int,
                gates_executed: int,
                qobj_config: {
                    n_qubits: int,
                    max_credits: int,
                    memory_slots: int,
                    memory: bool,
                    shots: int,
                    type: "string"
                },
                num_circuits: int,
                partial_validation: bool
            },
            resultTime: float
        }
        timePerStep: {
            CREATING: Date,
            ...
        },
        ip: {
            ip: "string",
            city: "string",
            country: "string",
            continent: "string"
        },
        hubInfo: {
            hub: {
                name: "string",
                priority: int
            },
            group: {
                name: "string",
                priority: int
            },
            project: {
                name: "string",
                priority: int
            }
        },
        codeId: "string",
        endDate: Date,
        cost: float,
        id: "string",
        userId: "string"
    }
    ...
]
```

### /jobs/{job_id}/cancel
Cancel the given job
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
```
### /jobs/{job_id}/status
Get the information on a specific job if you know the job's unique id
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
{
    kind: "string",
    backend: {
        id: "string",
        name: "string"
    },
    status: "string",
    creationDate: Date,
    hubInfo: {},
    endDate: Date,
    id: "string",
    userId: "string",
    name: "string"
}
```
### /jobs/{job_id}/properties

### /jobs/{job_id}/jobDataUploaded

### /jobs/{job_id}/resultDownloaded

### /job/{job_id}s/jobDownloadUrl

### /jobs/{job_id}/resultDownloadUrl

### /jobs/{job_id}/jobUploadUrl

## /network
### GET /network
#### Purpose
Get all the groups, devices, and networks which a user belongs to
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
[
    {
        name: "string",
        title: "string",
        description: "string",
        creationDate: Date,
        deleted: bool,
        private: bool,
        licenseNotRequired: bool,
        isDefault: bool,
        analytics: bool,
        class: "string",
        id: "string",
        groups: {
            groupName: {
                name: "string",
                title: "string",
                description: "string",
                creationDate: Date,
                deleted: bool,
                projects: {
                    projectName: {
                        name: "string",
                        title: "string",
                        description: "string",
                        creationDate: Date,
                        deleted: bool,
                        devices: {
                            deviceId: {
                                priority: int,
                                name: "string",
                                deleted: bool,
                                specificConfiguration: {}
                            }
                            ...
                        }
                        ...
                    }
                    ...
                }
            }
            ...
        }
    }
    ...
]
```

## /users
### POST /users/login
#### Purpose
Login with an email and a password.
#### Request Body
```js
{
    email: "string",
    password: "string"
}
```
#### Response Body
?
### POST /users/loginWithToken
#### Purpose
Login with the API using an IBM api token generated. This token is obtained through the IBM quantum computing website under the account section.
#### Request Body
```js
{
    apiToken: "string"  // api token linked to a user's IBM cloud id
}
```
#### Response Body
```js
{
    id: "string",       // access_token used to authenticate further requests
    ttl: int,           // time to logout
    created: Date,      // IBM account creation timestamp
    userId: "string",   // IBM unique user id
}
```
### GET /users/me
#### Purpose
Request account information
#### Request Body
```js
{
    access_token: "string"  // access_token obtained from appropriate login mechanisms
}
```
#### Response Body
```js
{
    firstName: "string",
    lastName: "string",
    userTypeId: "string",
    institution: "string",
    status: "string",
    blocked: "string",
    subscriptions: {
        updates: int,
        surveys: int
    },
    additionalData: {
        tutorialPagesCompleted: {},
        tutorialCompleted: bool,
        tutorialVisited: bool,
        termsAndConditions: bool,
        invitation: {
            familiarity: "string",
            description: "string",
            institution: "institution"
        }
    },
    codesLastVersion: {},
    creationDate: Date,
    iqxPreferences: ?,
    emailVerified: bool,
    username: "string",
    id: "string",
    lastModified: Date,
    credit: {
        remaining: int,
        promotional: int,
        promotionalCodesUsed: [],
        maxUserType: int
    },
    ibmQNetwork: bool,
    urls: {}
    dpl: {
        blocked: bool,
        checked: bool,
        wordsFound: {},
        results: {}
    },
    userType: {
        type: "string",
        default: bool,
        priority: bool,
        id: "string",
        credit: {
            credits: int
        },
        delete: bool
    },
    groups: [],
    roles: []
}
```
