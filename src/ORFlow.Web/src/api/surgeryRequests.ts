const apiBaseUrl = 'http://localhost:5008'

export type RequestStatus = 0 | 1 | 2 | 3 | 4

export type CreateSurgeryRequestRequest = {
    patientId: string
    surgeonId: string
    operatingRoomId: string
    procedureName: string
    requestedStartTime: string
    requestedEndTime: string
}

export type SurgeryRequest = {
    surgeryRequestId: string
    patientId: string
    surgeonId: string
    operatingRoomId: string
    procedureName: string
    requestedStartTime: string
    requestedEndTime: string
    requestStatus: RequestStatus
}

export async function createSurgeryRequest(
    request: CreateSurgeryRequestRequest,
): Promise<SurgeryRequest> {
    const response = await fetch(`${apiBaseUrl}/surgery-requests`, {
        method: 'POST',
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
    })

    if (!response.ok) {
        throw new Error('Could not create the surgery request.')
    }

    return response.json() as Promise<SurgeryRequest>
}

export async function getSurgeryRequest(
    surgeryRequestId: string,
): Promise<SurgeryRequest> {
    const response = await fetch(
        `${apiBaseUrl}/surgery-requests/${surgeryRequestId}`,
        {
            method: 'GET',
            credentials: 'include',
        },
    )

    if (!response.ok) {
        throw new Error('Could not retrieve the surgery request.')
    }

    return response.json() as Promise<SurgeryRequest>
}

export async function approveSurgeryRequest(
    surgeryRequestId: string,
): Promise<SurgeryRequest> {
    return updateSurgeryRequest(surgeryRequestId, 'approve')
}

export async function scheduleSurgeryRequest(
    surgeryRequestId: string,
): Promise<SurgeryRequest> {
    return updateSurgeryRequest(surgeryRequestId, 'schedule')
}

export async function completeSurgeryRequest(
    surgeryRequestId: string,
): Promise<SurgeryRequest> {
    return updateSurgeryRequest(surgeryRequestId, 'complete')
}

export async function archiveSurgeryRequest(
    surgeryRequestId: string,
): Promise<SurgeryRequest> {
    return updateSurgeryRequest(surgeryRequestId, 'archive')
}

async function updateSurgeryRequest(
    surgeryRequestId: string,
    action: 'approve' | 'schedule' | 'complete' | 'archive',
): Promise<SurgeryRequest> {
    const response = await fetch(
        `${apiBaseUrl}/surgery-requests/${surgeryRequestId}/${action}`,
        {
            method: 'POST',
            credentials: 'include',
        },
    )

    if (!response.ok) {
        if (response.status === 409) {
            throw new Error('Scheduling conflict detected.')
        }

        throw new Error(`Could not ${action} the surgery request.`)
    }

    return response.json() as Promise<SurgeryRequest>
}
