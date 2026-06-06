import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import ProfileTab from '../ProfileTab'

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(),
}))

vi.mock('@/lib/shopbeApi', () => ({
  shopbeApi: {
    users: {
      getMe: vi.fn(),
      sync: vi.fn(),
    },
  },
}))

vi.mock('@/lib/toast', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn(),
  },
}))

const { useSession } = await import('next-auth/react')
const { shopbeApi } = await import('@/lib/shopbeApi')
const { toast } = await import('@/lib/toast')

const mockSession = {
  data: {
    accessToken: 'test-token',
    expires: '2099-01-01T00:00:00.000Z',
    user: { name: 'John Doe', email: 'john@example.com', image: '' },
  },
  status: 'authenticated' as const,
  update: vi.fn(),
}

const mockProfile = {
  id: 'user-1',
  keycloakId: 'kc-1',
  email: 'john@example.com',
  fullName: 'John Doe',
  phoneNumber: '0912345678',
  gender: 'male',
  birthday: '1990-01-01T00:00:00Z',
  language: 'English (US)',
  country: 'Vietnam',
  avatarUrl: '',
  role: 0,
  status: 0,
  createdAt: '2024-01-01T00:00:00Z',
  updatedAt: '2024-01-01T00:00:00Z',
}

describe('ProfileTab', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useSession).mockReturnValue(mockSession)
  })

  it('shows loading spinner while fetching profile', () => {
    vi.mocked(shopbeApi.users.getMe).mockReturnValue(new Promise(() => {}))
    render(<ProfileTab />)
    expect(screen.getByText(/loading your profile/i)).toBeInTheDocument()
  })

  it('renders profile form with fetched data', async () => {
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    expect(screen.getByDisplayValue('john@example.com')).toBeInTheDocument()
    expect(screen.getByDisplayValue('0912345678')).toBeInTheDocument()
    expect(screen.getByDisplayValue('1990-01-01')).toBeInTheDocument()
  })

  it('falls back to session data when fetch fails', async () => {
    vi.mocked(shopbeApi.users.getMe).mockRejectedValue(new Error('Network error'))

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    expect(screen.getByDisplayValue('john@example.com')).toBeInTheDocument()
  })

  it('enables save button when form changes', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    expect(saveBtn).toBeDisabled()

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    expect(saveBtn).toBeEnabled()
  })

  it('calls sync API on save and shows success toast', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)
    vi.mocked(shopbeApi.users.sync).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    await user.click(saveBtn)

    await waitFor(() => {
      expect(shopbeApi.users.sync).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({ fullName: 'Jane Doe', birthday: '1990-01-01' })
      )
    })

    expect(toast.success).toHaveBeenCalledWith('Profile updated successfully!')
  })

  it('converts empty birthday to null before sync', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue({
      ...mockProfile,
      birthday: '',
    })
    vi.mocked(shopbeApi.users.sync).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    await user.click(saveBtn)

    await waitFor(() => {
      expect(shopbeApi.users.sync).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({ birthday: null })
      )
    })
  })

  it('shows error message from API on save failure', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)
    vi.mocked(shopbeApi.users.sync).mockRejectedValue(new Error('API 400 Bad Request: validation failed'))

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    await user.click(saveBtn)

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith('API 400 Bad Request: validation failed')
    })
  })

  it('shows fallback message when error has no message', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)
    vi.mocked(shopbeApi.users.sync).mockRejectedValue('string error')

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    await user.click(saveBtn)

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith('Failed to update profile. Please try again.')
    })
  })

  it('shows error toast with API message when save fails', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)
    vi.mocked(shopbeApi.users.sync).mockRejectedValue(new Error('API error'))

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const nameInput = screen.getByDisplayValue('John Doe')
    await user.clear(nameInput)
    await user.type(nameInput, 'Jane Doe')

    const saveBtn = screen.getByRole('button', { name: /save changes/i })
    await user.click(saveBtn)

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith('API error')
    })
  })

  it('resets form on reset button click', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const phoneInput = screen.getByDisplayValue('0912345678')
    await user.clear(phoneInput)
    await user.type(phoneInput, '0999999999')

    const resetBtn = screen.getByRole('button', { name: /reset/i })
    await user.click(resetBtn)

    expect(screen.getByDisplayValue('0912345678')).toBeInTheDocument()
  })

  it('calculates completion rate correctly', async () => {
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByText('100%')).toBeInTheDocument()
    })

    expect(screen.getByText('Complete!')).toBeInTheDocument()
  })

  it('shows incomplete profile when fields are missing', async () => {
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue({
      ...mockProfile,
      phoneNumber: '',
      birthday: '',
    })

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByText('Incomplete')).toBeInTheDocument()
    })

    expect(screen.getByText('50%')).toBeInTheDocument()
  })

  it('allows gender selection', async () => {
    const user = userEvent.setup()
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    const femaleBtn = screen.getByRole('button', { name: 'Female' })
    await user.click(femaleBtn)

    expect(femaleBtn).toHaveClass('border-brand')
  })

  it('disables save and reset when no changes made', async () => {
    vi.mocked(shopbeApi.users.getMe).mockResolvedValue(mockProfile)

    render(<ProfileTab />)

    await waitFor(() => {
      expect(screen.getByDisplayValue('John Doe')).toBeInTheDocument()
    })

    expect(screen.getByRole('button', { name: /save changes/i })).toBeDisabled()
    expect(screen.getByRole('button', { name: /reset/i })).toBeDisabled()
  })
})
