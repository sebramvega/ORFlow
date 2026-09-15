const apiBaseUrl = 'http://localhost:5008'

export type CurrentUser = {
  email: string
  roles: string[]
}

export async function login(
  email: string,
  password: string,
): Promise<void> {
  const response = await fetch(
    `${apiBaseUrl}/auth/login?useCookies=true`,
    {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email,
        password,
      }),
    },
  )

  if (!response.ok) {
    throw new Error('Login failed.')
  }
}

export async function getCurrentUser(): Promise<CurrentUser | null> {
  const response = await fetch(`${apiBaseUrl}/auth/me`, {
    credentials: 'include',
  })

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw new Error('Could not retrieve the current user.')
  }

  return response.json() as Promise<CurrentUser>
}

export async function logout(): Promise<void> {
  const response = await fetch(`${apiBaseUrl}/auth/logout`, {
    method: 'POST',
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error('Logout failed.')
  }
}

