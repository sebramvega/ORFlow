import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { getCurrentUser, login, logout } from './api/auth'
import type { CurrentUser } from './api/auth'
import {
  approveSurgeryRequest,
  archiveSurgeryRequest,
  completeSurgeryRequest,
  createSurgeryRequest,
  getSurgeryRequest,
  scheduleSurgeryRequest,
} from './api/surgeryRequests'
import type {
  RequestStatus,
  SurgeryRequest,
} from './api/surgeryRequests'
import './App.css'

function getStatusName(status: RequestStatus): string {
  const statusNames: Record<RequestStatus, string> = {
    0: 'Submitted',
    1: 'Approved',
    2: 'Scheduled',
    3: 'Completed',
    4: 'Archived',
  }

  return statusNames[status]
}

function App() {
  const [currentUser, setCurrentUser] =
    useState<CurrentUser | null>(null)

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isCheckingAuth, setIsCheckingAuth] = useState(true)
  const [error, setError] = useState('')

  const [patientId, setPatientId] = useState('')
  const [surgeonId, setSurgeonId] = useState('')
  const [operatingRoomId, setOperatingRoomId] = useState('')
  const [procedureName, setProcedureName] = useState('')
  const [requestedStartTime, setRequestedStartTime] = useState('')
  const [requestedEndTime, setRequestedEndTime] = useState('')

  const [createdRequest, setCreatedRequest] =
    useState<SurgeryRequest | null>(null)

  const [lookupId, setLookupId] = useState('')
  const [retrievedRequest, setRetrievedRequest] =
    useState<SurgeryRequest | null>(null)

  useEffect(() => {
    async function checkAuthentication() {
      try {
        const user = await getCurrentUser()
        setCurrentUser(user)
      } catch {
        setError('Could not connect to the ORFlow API.')
      } finally {
        setIsCheckingAuth(false)
      }
    }

    void checkAuthentication()
  }, [])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')

    try {
      await login(email, password)

      const user = await getCurrentUser()
      setCurrentUser(user)
    } catch {
      setError('Login failed. Check your email and password.')
    }
  }

  async function handleLogout() {
    setError('')

    try {
      await logout()
      setCurrentUser(null)
      setEmail('')
      setPassword('')
      setCreatedRequest(null)
      setRetrievedRequest(null)
      setLookupId('')
    } catch {
      setError('Logout failed.')
    }
  }

  async function handleCreateSurgeryRequest(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setError('')

    try {
      const surgeryRequest = await createSurgeryRequest({
        patientId,
        surgeonId,
        operatingRoomId,
        procedureName,
        requestedStartTime: new Date(requestedStartTime).toISOString(),
        requestedEndTime: new Date(requestedEndTime).toISOString(),
      })

      setCreatedRequest(surgeryRequest)
      setLookupId(surgeryRequest.surgeryRequestId)
    } catch {
      setError('Could not create the surgery request.')
    }
  }

  async function handleLookup(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()
    setError('')
    setRetrievedRequest(null)

    try {
      const surgeryRequest = await getSurgeryRequest(lookupId)
      setRetrievedRequest(surgeryRequest)
    } catch {
      setError('Could not retrieve the surgery request.')
    }
  }

  async function handleWorkflowAction(
    action: (id: string) => Promise<SurgeryRequest>,
  ) {
    if (!retrievedRequest) {
      return
    }

    setError('')

    try {
      const updatedRequest =
        await action(retrievedRequest.surgeryRequestId)

      setRetrievedRequest(updatedRequest)
    } catch (error) {
      if (error instanceof Error) {
        setError(error.message)
      } else {
        setError('Could not update the surgery request.')
      }
    }
  }

  function renderSchedulerAction() {
    if (
      !retrievedRequest ||
      !currentUser?.roles.includes('Scheduler')
    ) {
      return null
    }

    switch (retrievedRequest.requestStatus) {
      case 0:
        return (
          <button
            type="button"
            onClick={() =>
              void handleWorkflowAction(approveSurgeryRequest)
            }
          >
            Approve request
          </button>
        )

      case 1:
        return (
          <button
            type="button"
            onClick={() =>
              void handleWorkflowAction(scheduleSurgeryRequest)
            }
          >
            Schedule request
          </button>
        )

      case 2:
        return (
          <button
            type="button"
            onClick={() =>
              void handleWorkflowAction(completeSurgeryRequest)
            }
          >
            Complete request
          </button>
        )

      case 3:
        return (
          <button
            type="button"
            onClick={() =>
              void handleWorkflowAction(archiveSurgeryRequest)
            }
          >
            Archive request
          </button>
        )

      case 4:
        return <p>This request has completed its workflow.</p>
    }
  }

  if (isCheckingAuth) {
    return (
      <main className="app">
        <section className="app-card">
          <h1>ORFlow</h1>
          <p>Checking authentication...</p>
        </section>
      </main>
    )
  }

  return (
    <main className="app">
      <section className="app-card">
        <h1>ORFlow</h1>
        <p>Operating Room Scheduling & Resource Coordination</p>

        {currentUser ? (
          <div>
            <h2>Signed in</h2>
            <p>{currentUser.email}</p>
            <p>
              Roles:{' '}
              {currentUser.roles.length > 0
                ? currentUser.roles.join(', ')
                : 'None'}
            </p>

            <button type="button" onClick={handleLogout}>
              Sign out
            </button>

            {currentUser.roles.includes('Surgeon') && (
              <section className="workflow-section">
                <h2>Create Surgery Request</h2>

                <form onSubmit={handleCreateSurgeryRequest}>
                  <label htmlFor="patientId">Patient ID</label>
                  <input
                    id="patientId"
                    value={patientId}
                    onChange={(event) =>
                      setPatientId(event.target.value)
                    }
                    placeholder="Patient GUID"
                    required
                  />

                  <label htmlFor="surgeonId">Surgeon ID</label>
                  <input
                    id="surgeonId"
                    value={surgeonId}
                    onChange={(event) =>
                      setSurgeonId(event.target.value)
                    }
                    placeholder="Surgeon GUID"
                    required
                  />

                  <label htmlFor="operatingRoomId">
                    Operating Room ID
                  </label>
                  <input
                    id="operatingRoomId"
                    value={operatingRoomId}
                    onChange={(event) =>
                      setOperatingRoomId(event.target.value)
                    }
                    placeholder="Operating Room GUID"
                    required
                  />

                  <label htmlFor="procedureName">Procedure</label>
                  <input
                    id="procedureName"
                    value={procedureName}
                    onChange={(event) =>
                      setProcedureName(event.target.value)
                    }
                    required
                  />

                  <label htmlFor="requestedStartTime">
                    Start time
                  </label>
                  <input
                    id="requestedStartTime"
                    type="datetime-local"
                    value={requestedStartTime}
                    onChange={(event) =>
                      setRequestedStartTime(event.target.value)
                    }
                    required
                  />

                  <label htmlFor="requestedEndTime">
                    End time
                  </label>
                  <input
                    id="requestedEndTime"
                    type="datetime-local"
                    value={requestedEndTime}
                    onChange={(event) =>
                      setRequestedEndTime(event.target.value)
                    }
                    required
                  />

                  <button type="submit">
                    Create request
                  </button>
                </form>

                {createdRequest && (
                  <div className="request-result">
                    <h3>Request created</h3>
                    <p>
                      <strong>ID:</strong>{' '}
                      {createdRequest.surgeryRequestId}
                    </p>
                    <p>
                      <strong>Procedure:</strong>{' '}
                      {createdRequest.procedureName}
                    </p>
                    <p>
                      <strong>Status:</strong>{' '}
                      {getStatusName(createdRequest.requestStatus)}
                    </p>
                  </div>
                )}
              </section>
            )}

            <section className="workflow-section">
              <h2>Find Surgery Request</h2>

              <form onSubmit={handleLookup}>
                <label htmlFor="lookupId">
                  Surgery Request ID
                </label>
                <input
                  id="lookupId"
                  value={lookupId}
                  onChange={(event) =>
                    setLookupId(event.target.value)
                  }
                  placeholder="Surgery request GUID"
                  required
                />

                <button type="submit">Find request</button>
              </form>

              {retrievedRequest && (
                <div className="request-result">
                  <h3>Surgery Request</h3>

                  <p>
                    <strong>ID:</strong>{' '}
                    {retrievedRequest.surgeryRequestId}
                  </p>

                  <p>
                    <strong>Procedure:</strong>{' '}
                    {retrievedRequest.procedureName}
                  </p>

                  <p>
                    <strong>Status:</strong>{' '}
                    {getStatusName(retrievedRequest.requestStatus)}
                  </p>

                  <p>
                    <strong>Patient ID:</strong>{' '}
                    {retrievedRequest.patientId}
                  </p>

                  <p>
                    <strong>Surgeon ID:</strong>{' '}
                    {retrievedRequest.surgeonId}
                  </p>

                  <p>
                    <strong>Operating Room ID:</strong>{' '}
                    {retrievedRequest.operatingRoomId}
                  </p>

                  <p>
                    <strong>Start:</strong>{' '}
                    {new Date(
                      retrievedRequest.requestedStartTime,
                    ).toLocaleString()}
                  </p>

                  <p>
                    <strong>End:</strong>{' '}
                    {new Date(
                      retrievedRequest.requestedEndTime,
                    ).toLocaleString()}
                  </p>

                  {renderSchedulerAction()}
                </div>
              )}
            </section>
          </div>
        ) : (
          <form onSubmit={handleLogin}>
            <h2>Sign in</h2>

            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />

            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />

            <button type="submit">Sign in</button>
          </form>
        )}

        {error && <p className="error-message">{error}</p>}
      </section>
    </main>
  )
}

export default App
