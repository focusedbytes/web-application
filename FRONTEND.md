# Frontend Guidelines - FocusedBytes

This document provides comprehensive guidelines for frontend development in the FocusedBytes project using SvelteKit 5 and Svelte 5.

## Table of Contents

1. [Technology Stack](#technology-stack)
2. [Project Structure](#project-structure)
3. [Svelte 5 Runes](#svelte-5-runes)
4. [Component Patterns](#component-patterns)
5. [Tailwind CSS v4](#tailwind-css-v4)
6. [State Management](#state-management)
7. [API Integration](#api-integration)
8. [Form Handling](#form-handling)
9. [TypeScript Conventions](#typescript-conventions)
10. [Route Structure](#route-structure)
11. [UI Components](#ui-components)
12. [Testing](#testing)
13. [Performance](#performance)
14. [Accessibility](#accessibility)
15. [Future Enhancements](#future-enhancements)

---

## Technology Stack

### Core Framework
- **SvelteKit 5** - Full-stack web framework
- **Svelte 5** - Reactive UI framework with new Runes syntax
- **TypeScript** - Type safety
- **Vite 6** - Build tool and dev server

### Styling
- **Tailwind CSS v4** - Utility-first CSS framework
- **PostCSS** - CSS processing with @tailwindcss/postcss
- **Autoprefixer** - Automatic vendor prefixes

### UI Libraries (Adopt NOW)
- **svelte-sonner** - Toast notifications
- **lucide-svelte** - Icon library
- **bits-ui** - Headless UI components (dialogs, dropdowns, etc.)

### Utilities (Adopt WITH Authentication or LATER)
- **date-fns** - Date formatting and manipulation
- **Zod** - Schema validation
- **Superforms** - Advanced form handling with validation

### Testing (Adopt LATER)
- **Vitest** - Unit testing framework
- **Testing Library** - Component testing utilities
- **Playwright** - E2E testing

### Future
- **i18n library** (TBD) - Internationalization support

---

## Project Structure

```
frontend/
├── src/
│   ├── lib/
│   │   ├── api/                 # API client functions
│   │   │   └── users.ts         # User API endpoints
│   │   ├── components/          # Reusable UI components
│   │   │   ├── Pagination.svelte
│   │   │   ├── UserTable.svelte
│   │   │   └── UserForm.svelte
│   │   ├── types/               # TypeScript type definitions
│   │   │   └── user.ts
│   │   ├── utils/               # Utility functions (future)
│   │   └── stores/              # Shared state stores (future)
│   ├── routes/                  # File-based routing
│   │   ├── +page.svelte         # Root page (redirects to /admin)
│   │   ├── +layout.svelte       # Root layout
│   │   └── admin/
│   │       ├── +page.svelte     # Admin dashboard
│   │       └── users/
│   │           ├── +page.svelte        # User list
│   │           ├── create/
│   │           │   └── +page.svelte    # Create user
│   │           └── [id]/
│   │               └── edit/
│   │                   └── +page.svelte # Edit user
│   ├── app.css                  # Global styles
│   ├── app.d.ts                 # Global type declarations
│   └── app.html                 # HTML template
├── static/                      # Static assets
├── package.json
├── tsconfig.json
├── vite.config.ts
├── postcss.config.js
└── tailwind.config.ts
```

---

## Svelte 5 Runes

Svelte 5 introduces **Runes** - a new syntax for reactivity. We use Runes throughout the project.

### Key Runes

#### `$state` - Reactive State

Create reactive variables that trigger updates when changed:

```svelte
<script lang="ts">
let count = $state(0);

function increment() {
    count++; // Component automatically re-renders
}
</script>

<button onclick={increment}>Count: {count}</button>
```

**Object and Array State:**
```svelte
<script lang="ts">
let user = $state({
    name: '',
    email: '',
    role: UserRole.User
});

function updateName(newName: string) {
    user.name = newName; // Triggers reactivity
}
</script>
```

#### `$props` - Component Props

Define component properties with TypeScript types:

```svelte
<script lang="ts">
interface Props {
    title: string;
    count?: number; // Optional
    onIncrement: () => void;
}

let { title, count = 0, onIncrement }: Props = $props();
</script>

<h1>{title}</h1>
<p>Count: {count}</p>
<button onclick={onIncrement}>Increment</button>
```

**Props Destructuring:**
```svelte
<script lang="ts">
let {
    user,
    isLoading = false,
    onSave
}: {
    user: User;
    isLoading?: boolean;
    onSave: (user: User) => void;
} = $props();
</script>
```

#### `$derived` - Computed Values

Create values that automatically update when dependencies change:

```svelte
<script lang="ts">
let firstName = $state('John');
let lastName = $state('Doe');

// Automatically recalculates when firstName or lastName changes
let fullName = $derived(`${firstName} ${lastName}`);

// Derived from props
let { users }: { users: User[] } = $props();
let activeUsers = $derived(users.filter(u => u.isActive));
</script>

<p>Full name: {fullName}</p>
<p>Active users: {activeUsers.length}</p>
```

#### `$effect` - Side Effects

Run side effects when dependencies change:

```svelte
<script lang="ts">
import { onMount } from 'svelte';

let count = $state(0);

// Runs whenever count changes
$effect(() => {
    console.log(`Count is now: ${count}`);
    document.title = `Count: ${count}`;
});

// For mount/cleanup, still use onMount
onMount(() => {
    console.log('Component mounted');

    return () => {
        console.log('Component unmounted');
    };
});
</script>
```

### Migration from Svelte 4

**OLD (Svelte 4):**
```svelte
<script lang="ts">
export let count: number;
let doubled: number;

$: doubled = count * 2;
$: console.log(count);
</script>
```

**NEW (Svelte 5):**
```svelte
<script lang="ts">
let { count }: { count: number } = $props();

let doubled = $derived(count * 2);

$effect(() => {
    console.log(count);
});
</script>
```

---

## Component Patterns

### Presentational Component Pattern

**Location:** `src/lib/components/UserTable.svelte`

```svelte
<script lang="ts">
import type { User } from '$lib/types/user';

interface Props {
    users: User[];
    onEdit: (userId: string) => void;
    onDelete: (userId: string) => void;
}

let { users, onEdit, onDelete }: Props = $props();
</script>

<div class="overflow-x-auto">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Email
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Role
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                </th>
            </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
            {#each users as user (user.id)}
                <tr class="hover:bg-gray-50 transition-colors">
                    <td class="px-6 py-4 whitespace-nowrap">
                        <div class="text-sm font-medium text-gray-900">{user.email}</div>
                        {#if user.phone}
                            <div class="text-sm text-gray-500">{user.phone}</div>
                        {/if}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full
                            {user.role === 'Admin' ? 'bg-purple-100 text-purple-800' : 'bg-gray-100 text-gray-800'}">
                            {user.role}
                        </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full
                            {user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                            {user.isActive ? 'Active' : 'Inactive'}
                        </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button
                            onclick={() => onEdit(user.id)}
                            class="text-indigo-600 hover:text-indigo-900 mr-4"
                        >
                            Edit
                        </button>
                        <button
                            onclick={() => onDelete(user.id)}
                            class="text-red-600 hover:text-red-900"
                        >
                            Delete
                        </button>
                    </td>
                </tr>
            {/each}
        </tbody>
    </table>
</div>
```

### Container Component Pattern

**Location:** `src/routes/admin/users/+page.svelte`

```svelte
<script lang="ts">
import { onMount } from 'svelte';
import { goto } from '$app/navigation';
import UserTable from '$lib/components/UserTable.svelte';
import Pagination from '$lib/components/Pagination.svelte';
import type { User } from '$lib/types/user';
import * as usersApi from '$lib/api/users';

// State
let users = $state<User[]>([]);
let isLoading = $state(true);
let error = $state<string | null>(null);
let currentPage = $state(1);
let totalPages = $state(1);
const pageSize = 20;

// Derived state
let hasUsers = $derived(users.length > 0);

// Load data on mount
onMount(async () => {
    await loadUsers();
});

// Functions
async function loadUsers() {
    isLoading = true;
    error = null;

    try {
        const response = await usersApi.getUsers(currentPage, pageSize);
        users = response.users;
        totalPages = response.totalPages;
    } catch (err) {
        error = err instanceof Error ? err.message : 'Failed to load users';
        console.error('Error loading users:', err);
    } finally {
        isLoading = false;
    }
}

function handleEdit(userId: string) {
    goto(`/admin/users/${userId}/edit`);
}

async function handleDelete(userId: string) {
    if (!confirm('Are you sure you want to delete this user?')) return;

    try {
        await usersApi.deleteUser(userId);
        await loadUsers(); // Reload list
    } catch (err) {
        console.error('Error deleting user:', err);
        // TODO: Show toast notification
    }
}

function handlePageChange(page: number) {
    currentPage = page;
    loadUsers();
}
</script>

<div class="container mx-auto px-4 py-8">
    <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-800">Users</h1>
        <a href="/admin/users/create" class="btn btn-primary">
            Create User
        </a>
    </div>

    {#if isLoading}
        <div class="text-center py-8">Loading...</div>
    {:else if error}
        <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
            {error}
        </div>
    {:else if hasUsers}
        <UserTable
            {users}
            onEdit={handleEdit}
            onDelete={handleDelete}
        />

        <Pagination
            {currentPage}
            {totalPages}
            onPageChange={handlePageChange}
        />
    {:else}
        <div class="text-center py-8 text-gray-500">
            No users found. <a href="/admin/users/create" class="text-indigo-600">Create one?</a>
        </div>
    {/if}
</div>
```

### Form Component Pattern

**Location:** `src/lib/components/UserForm.svelte`

```svelte
<script lang="ts">
import { UserRole } from '$lib/types/user';
import type { User } from '$lib/types/user';

interface Props {
    user?: User; // Undefined for create, provided for edit
    onSubmit: (formData: FormData) => void;
    onCancel: () => void;
}

interface FormData {
    email: string;
    phone: string;
    password: string;
    role: UserRole;
}

let { user, onSubmit, onCancel }: Props = $props();

// Form state
let formData = $state<FormData>({
    email: user?.email || '',
    phone: user?.phone || '',
    password: '',
    role: user?.role || UserRole.User
});

let errors = $state<Partial<Record<keyof FormData, string>>>({});

// Computed
let isEditMode = $derived(!!user);
let isValid = $derived(
    (formData.email || formData.phone) &&
    (isEditMode || formData.password.length >= 6)
);

// Validation
function validateForm(): boolean {
    errors = {};

    if (!formData.email && !formData.phone) {
        errors.email = 'Email or phone is required';
        return false;
    }

    if (formData.email && !isValidEmail(formData.email)) {
        errors.email = 'Invalid email format';
        return false;
    }

    if (!isEditMode && formData.password.length < 6) {
        errors.password = 'Password must be at least 6 characters';
        return false;
    }

    return true;
}

function isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function handleSubmit(event: Event) {
    event.preventDefault();

    if (!validateForm()) return;

    onSubmit(formData);
}
</script>

<form onsubmit={handleSubmit} class="space-y-4">
    <div>
        <label for="email" class="block text-sm font-medium text-gray-700">
            Email
        </label>
        <input
            type="email"
            id="email"
            bind:value={formData.email}
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            placeholder="user@example.com"
        />
        {#if errors.email}
            <p class="mt-1 text-sm text-red-600">{errors.email}</p>
        {/if}
    </div>

    <div>
        <label for="phone" class="block text-sm font-medium text-gray-700">
            Phone (optional)
        </label>
        <input
            type="tel"
            id="phone"
            bind:value={formData.phone}
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            placeholder="+380501234567"
        />
        {#if errors.phone}
            <p class="mt-1 text-sm text-red-600">{errors.phone}</p>
        {/if}
    </div>

    <div>
        <label for="password" class="block text-sm font-medium text-gray-700">
            Password {isEditMode ? '(leave empty to keep current)' : ''}
        </label>
        <input
            type="password"
            id="password"
            bind:value={formData.password}
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
            placeholder="••••••••"
        />
        {#if errors.password}
            <p class="mt-1 text-sm text-red-600">{errors.password}</p>
        {/if}
    </div>

    <div>
        <label for="role" class="block text-sm font-medium text-gray-700">
            Role
        </label>
        <select
            id="role"
            bind:value={formData.role}
            class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
        >
            <option value={UserRole.User}>User</option>
            <option value={UserRole.Admin}>Admin</option>
        </select>
    </div>

    <div class="flex gap-4">
        <button
            type="submit"
            disabled={!isValid}
            class="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
        >
            {isEditMode ? 'Update' : 'Create'} User
        </button>
        <button
            type="button"
            onclick={onCancel}
            class="btn btn-secondary"
        >
            Cancel
        </button>
    </div>
</form>
```

---

## Tailwind CSS v4

We use **Tailwind CSS v4** with the new import syntax.

### Configuration

**postcss.config.js:**
```javascript
export default {
    plugins: {
        '@tailwindcss/postcss': {},
        autoprefixer: {}
    }
};
```

**tailwind.config.ts:**
```typescript
import type { Config } from 'tailwindcss';

export default {
    content: ['./src/**/*.{html,js,svelte,ts}'],
    theme: {
        extend: {}
    },
    plugins: []
} satisfies Config;
```

**src/app.css:**
```css
@import "tailwindcss";

/* Global styles */
body {
    @apply bg-gray-100 text-gray-900;
}

/* Custom utilities */
.btn {
    @apply px-4 py-2 rounded-md font-medium transition-colors;
}

.btn-primary {
    @apply bg-indigo-600 text-white hover:bg-indigo-700;
}

.btn-secondary {
    @apply bg-gray-200 text-gray-800 hover:bg-gray-300;
}

.btn-danger {
    @apply bg-red-600 text-white hover:bg-red-700;
}
```

### Utility Class Conventions

**DO:**
- Use utility classes for styling
- Group related utilities (spacing, colors, typography)
- Use Tailwind's responsive prefixes (sm:, md:, lg:)
- Use hover:, focus:, active: states
- Extract repeated patterns into custom classes

**DON'T:**
- Write custom CSS for one-off styles
- Use inline styles
- Create wrapper divs just for styling

**Example:**
```svelte
<!-- Good -->
<button class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors">
    Click me
</button>

<!-- Bad -->
<button style="padding: 1rem; background: indigo;">Click me</button>
```

### Responsive Design

```svelte
<div class="
    grid
    grid-cols-1
    sm:grid-cols-2
    md:grid-cols-3
    lg:grid-cols-4
    gap-4
">
    <!-- Cards -->
</div>
```

### Dark Mode (Future)

Tailwind supports dark mode with `dark:` prefix:

```svelte
<div class="bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100">
    Content
</div>
```

---

## State Management

### Local Component State

Use `$state` for component-specific state:

```svelte
<script lang="ts">
let count = $state(0);
let user = $state<User | null>(null);
let isLoading = $state(false);
</script>
```

### Shared State (Future)

For state shared across components, create stores in `src/lib/stores/`:

**src/lib/stores/auth.svelte.ts:**
```typescript
// Using Svelte 5 runes in .svelte.ts files
let currentUser = $state<User | null>(null);
let isAuthenticated = $derived(currentUser !== null);

export function useAuth() {
    return {
        get user() { return currentUser; },
        get isAuthenticated() { return isAuthenticated; },
        setUser: (user: User | null) => {
            currentUser = user;
        },
        logout: () => {
            currentUser = null;
        }
    };
}
```

**Usage:**
```svelte
<script lang="ts">
import { useAuth } from '$lib/stores/auth.svelte';

const auth = useAuth();
</script>

{#if auth.isAuthenticated}
    <p>Welcome, {auth.user?.email}</p>
    <button onclick={() => auth.logout()}>Logout</button>
{:else}
    <a href="/login">Login</a>
{/if}
```

### SvelteKit Page State

Use `page` from `$app/state` for URL params and data:

```svelte
<script lang="ts">
import { page } from '$app/state';

let userId = $derived(page.params.id!);
let searchQuery = $derived(page.url.searchParams.get('q') || '');
</script>

<p>Editing user: {userId}</p>
<p>Search query: {searchQuery}</p>
```

---

## API Integration

### API Client Pattern

Centralize API calls in `src/lib/api/`:

**src/lib/api/users.ts:**
```typescript
import type {
    User,
    UserDetail,
    CreateUserRequest,
    UpdateUserRequest,
    UpdateAccountRequest,
    UserListResponse
} from '$lib/types/user';

const API_BASE = 'http://localhost:5093/api/admin/users';

async function handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const error = await response.json().catch(() => ({ message: 'Request failed' }));
        throw new Error(error.message || `HTTP ${response.status}`);
    }
    return response.json();
}

export async function getUsers(
    page: number = 1,
    pageSize: number = 20,
    includeDeleted: boolean = false
): Promise<UserListResponse> {
    const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        includeDeleted: includeDeleted.toString()
    });

    const response = await fetch(`${API_BASE}?${params}`);
    return handleResponse<UserListResponse>(response);
}

export async function getUserById(id: string): Promise<UserDetail> {
    const response = await fetch(`${API_BASE}/${id}`);
    return handleResponse<UserDetail>(response);
}

export async function createUser(request: CreateUserRequest): Promise<string> {
    const response = await fetch(API_BASE, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
    });
    return handleResponse<string>(response);
}

export async function updateUser(id: string, request: UpdateUserRequest): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
    });
    await handleResponse<void>(response);
}

export async function updateAccount(id: string, request: UpdateAccountRequest): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}/account`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
    });
    await handleResponse<void>(response);
}

export async function updateUserStatus(id: string, isActive: boolean): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ isActive })
    });
    await handleResponse<void>(response);
}

export async function deleteUser(id: string): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}`, {
        method: 'DELETE'
    });
    await handleResponse<void>(response);
}
```

### Error Handling

**Current (Basic):**
```svelte
<script lang="ts">
let error = $state<string | null>(null);

async function loadData() {
    try {
        const data = await api.getData();
        // ...
    } catch (err) {
        error = err instanceof Error ? err.message : 'An error occurred';
        console.error('Error:', err);
    }
}
</script>

{#if error}
    <div class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
        {error}
    </div>
{/if}
```

**Future (with svelte-sonner):**
```svelte
<script lang="ts">
import { toast } from 'svelte-sonner';

async function loadData() {
    try {
        const data = await api.getData();
        toast.success('Data loaded successfully');
    } catch (err) {
        toast.error(err instanceof Error ? err.message : 'An error occurred');
        console.error('Error:', err);
    }
}
</script>
```

### Loading States

```svelte
<script lang="ts">
let isLoading = $state(false);

async function fetchData() {
    isLoading = true;
    try {
        await api.getData();
    } finally {
        isLoading = false;
    }
}
</script>

{#if isLoading}
    <div class="flex items-center justify-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    </div>
{:else}
    <!-- Content -->
{/if}
```

---

## Form Handling

### Basic Forms (Current)

```svelte
<script lang="ts">
interface FormData {
    email: string;
    password: string;
}

let formData = $state<FormData>({ email: '', password: '' });
let errors = $state<Partial<Record<keyof FormData, string>>>({});

function handleSubmit(event: Event) {
    event.preventDefault();

    if (!validate()) return;

    // Submit logic
}

function validate(): boolean {
    errors = {};

    if (!formData.email) {
        errors.email = 'Email is required';
        return false;
    }

    return true;
}
</script>

<form onsubmit={handleSubmit}>
    <input type="email" bind:value={formData.email} />
    {#if errors.email}
        <span class="error">{errors.email}</span>
    {/if}

    <button type="submit">Submit</button>
</form>
```

### Advanced Forms (Future - Superforms + Zod)

**src/lib/schemas/user.ts:**
```typescript
import { z } from 'zod';

export const createUserSchema = z.object({
    email: z.string().email('Invalid email').optional(),
    phone: z.string().regex(/^\+380\d{9}$/, 'Invalid phone').optional(),
    password: z.string().min(6, 'Password must be at least 6 characters'),
    role: z.enum(['User', 'Admin'])
}).refine(data => data.email || data.phone, {
    message: 'Email or phone is required',
    path: ['email']
});

export type CreateUserSchema = z.infer<typeof createUserSchema>;
```

**Component with Superforms:**
```svelte
<script lang="ts">
import { superForm } from 'sveltekit-superforms/client';
import { createUserSchema } from '$lib/schemas/user';

let { data } = $props(); // From +page.server.ts

const { form, errors, enhance } = superForm(data.form, {
    validators: createUserSchema,
    onUpdate: ({ form }) => {
        if (form.valid) {
            // Handle success
        }
    }
});
</script>

<form method="POST" use:enhance>
    <input name="email" bind:value={$form.email} />
    {#if $errors.email}
        <span class="error">{$errors.email}</span>
    {/if}

    <button type="submit">Submit</button>
</form>
```

---

## TypeScript Conventions

### Type Definitions

Keep all types in `src/lib/types/`:

**src/lib/types/user.ts:**
```typescript
export enum UserRole {
    User = 'User',
    Admin = 'Admin'
}

export interface User {
    id: string;
    email: string;
    phone: string | null;
    role: UserRole;
    isActive: boolean;
    lastLoginAt: string | null;
    createdAt: string;
}

export interface UserDetail extends User {
    // Additional fields for detail view
}

export interface CreateUserRequest {
    email?: string;
    phone?: string;
    password: string;
    role: UserRole;
}

export interface UpdateUserRequest {
    role: UserRole;
}

export interface UpdateAccountRequest {
    email?: string;
    phone?: string;
    password?: string;
}

export interface UserListResponse {
    users: User[];
    currentPage: number;
    totalPages: number;
    totalCount: number;
}
```

### Component Props Types

Always define explicit interfaces for props:

```svelte
<script lang="ts">
import type { User } from '$lib/types/user';

interface Props {
    users: User[];
    isLoading?: boolean;
    onSelect: (user: User) => void;
}

let { users, isLoading = false, onSelect }: Props = $props();
</script>
```

### Generic Types

```typescript
export interface ApiResponse<T> {
    data: T;
    error: string | null;
    isLoading: boolean;
}

export interface PaginatedResponse<T> {
    items: T[];
    currentPage: number;
    totalPages: number;
    totalCount: number;
}
```

---

## Route Structure

SvelteKit uses file-based routing:

### Route Files

- `+page.svelte` - Page component
- `+page.ts` - Load function (runs on both client and server)
- `+page.server.ts` - Server-only load function
- `+layout.svelte` - Layout component
- `+layout.ts` - Layout load function
- `+error.svelte` - Error page

### Current Routes

```
/                           → Redirects to /admin
/admin                      → Admin dashboard
/admin/users                → User list with pagination
/admin/users/create         → Create user form
/admin/users/[id]/edit      → Edit user form
```

### Route Parameters

```svelte
<!-- src/routes/admin/users/[id]/edit/+page.svelte -->
<script lang="ts">
import { page } from '$app/state';

let userId = $derived(page.params.id!);
</script>

<h1>Edit User {userId}</h1>
```

### Navigation

```svelte
<script lang="ts">
import { goto } from '$app/navigation';

function goToUsers() {
    goto('/admin/users');
}

function goToEditUser(userId: string) {
    goto(`/admin/users/${userId}/edit`);
}
</script>

<!-- Or use links -->
<a href="/admin/users">Go to Users</a>
<a href="/admin/users/{userId}/edit">Edit</a>
```

### Layout Pattern

**src/routes/+layout.svelte:**
```svelte
<script lang="ts">
import '../app.css';
</script>

<div class="min-h-screen">
    <nav class="bg-white shadow-sm">
        <div class="container mx-auto px-4">
            <div class="flex justify-between items-center h-16">
                <a href="/" class="text-xl font-bold">FocusedBytes</a>
                <div class="space-x-4">
                    <a href="/admin" class="text-gray-700 hover:text-gray-900">Dashboard</a>
                    <a href="/admin/users" class="text-gray-700 hover:text-gray-900">Users</a>
                </div>
            </div>
        </div>
    </nav>

    <main class="container mx-auto px-4 py-8">
        <slot />
    </main>
</div>
```

---

## UI Components

### Toast Notifications (svelte-sonner)

**Installation:**
```bash
npm install svelte-sonner
```

**Setup in +layout.svelte:**
```svelte
<script lang="ts">
import { Toaster } from 'svelte-sonner';
</script>

<Toaster />
<slot />
```

**Usage:**
```svelte
<script lang="ts">
import { toast } from 'svelte-sonner';

async function saveUser() {
    try {
        await api.createUser(formData);
        toast.success('User created successfully');
        goto('/admin/users');
    } catch (err) {
        toast.error('Failed to create user');
    }
}
</script>

<button onclick={saveUser}>Save</button>
```

### Icons (lucide-svelte)

**Installation:**
```bash
npm install lucide-svelte
```

**Usage:**
```svelte
<script lang="ts">
import { User, Edit, Trash, Plus } from 'lucide-svelte';
</script>

<button class="flex items-center gap-2">
    <Plus size={16} />
    Create User
</button>

<button>
    <Edit size={16} />
</button>

<button>
    <Trash size={16} />
</button>
```

### Dialogs (bits-ui)

**Installation:**
```bash
npm install bits-ui
```

**Delete Confirmation Dialog:**
```svelte
<script lang="ts">
import { Dialog } from 'bits-ui';

let dialogOpen = $state(false);
let userToDelete = $state<User | null>(null);

function openDeleteDialog(user: User) {
    userToDelete = user;
    dialogOpen = true;
}

async function confirmDelete() {
    if (!userToDelete) return;

    try {
        await api.deleteUser(userToDelete.id);
        toast.success('User deleted');
        dialogOpen = false;
        await loadUsers();
    } catch (err) {
        toast.error('Failed to delete user');
    }
}
</script>

<Dialog.Root bind:open={dialogOpen}>
    <Dialog.Trigger>
        <!-- Trigger button -->
    </Dialog.Trigger>

    <Dialog.Portal>
        <Dialog.Overlay class="fixed inset-0 bg-black/50" />
        <Dialog.Content class="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white rounded-lg p-6 max-w-md w-full">
            <Dialog.Title class="text-lg font-semibold mb-4">
                Delete User
            </Dialog.Title>
            <Dialog.Description class="text-gray-600 mb-6">
                Are you sure you want to delete {userToDelete?.email}? This action cannot be undone.
            </Dialog.Description>

            <div class="flex justify-end gap-4">
                <Dialog.Close class="btn btn-secondary">
                    Cancel
                </Dialog.Close>
                <button onclick={confirmDelete} class="btn btn-danger">
                    Delete
                </button>
            </div>
        </Dialog.Content>
    </Dialog.Portal>
</Dialog.Root>
```

---

## Testing

### Unit Testing (Vitest)

**Installation:**
```bash
npm install -D vitest @testing-library/svelte @testing-library/jest-dom
```

**vite.config.ts:**
```typescript
import { defineConfig } from 'vitest/config';
import { sveltekit } from '@sveltejs/kit/vite';

export default defineConfig({
    plugins: [sveltekit()],
    test: {
        include: ['src/**/*.{test,spec}.{js,ts}'],
        globals: true,
        environment: 'jsdom'
    }
});
```

**Example Test:**
```typescript
// src/lib/components/Pagination.test.ts
import { render, fireEvent } from '@testing-library/svelte';
import { describe, it, expect, vi } from 'vitest';
import Pagination from './Pagination.svelte';

describe('Pagination', () => {
    it('renders page numbers correctly', () => {
        const { getByText } = render(Pagination, {
            props: {
                currentPage: 1,
                totalPages: 5,
                onPageChange: vi.fn()
            }
        });

        expect(getByText('1')).toBeInTheDocument();
        expect(getByText('5')).toBeInTheDocument();
    });

    it('calls onPageChange when page is clicked', async () => {
        const onPageChange = vi.fn();
        const { getByText } = render(Pagination, {
            props: {
                currentPage: 1,
                totalPages: 5,
                onPageChange
            }
        });

        await fireEvent.click(getByText('2'));
        expect(onPageChange).toHaveBeenCalledWith(2);
    });
});
```

### Component Testing

```typescript
// src/lib/components/UserTable.test.ts
import { render } from '@testing-library/svelte';
import { describe, it, expect, vi } from 'vitest';
import UserTable from './UserTable.svelte';
import type { User } from '$lib/types/user';

describe('UserTable', () => {
    const mockUsers: User[] = [
        {
            id: '1',
            email: 'test@example.com',
            phone: null,
            role: 'User',
            isActive: true,
            lastLoginAt: null,
            createdAt: '2024-01-01T00:00:00Z'
        }
    ];

    it('renders users', () => {
        const { getByText } = render(UserTable, {
            props: {
                users: mockUsers,
                onEdit: vi.fn(),
                onDelete: vi.fn()
            }
        });

        expect(getByText('test@example.com')).toBeInTheDocument();
    });
});
```

### E2E Testing (Playwright)

**Installation:**
```bash
npm install -D @playwright/test
npx playwright install
```

**tests/users.spec.ts:**
```typescript
import { test, expect } from '@playwright/test';

test.describe('User Management', () => {
    test('should list users', async ({ page }) => {
        await page.goto('/admin/users');

        await expect(page.locator('h1')).toHaveText('Users');
        await expect(page.locator('table')).toBeVisible();
    });

    test('should create a user', async ({ page }) => {
        await page.goto('/admin/users/create');

        await page.fill('input[type="email"]', 'newuser@example.com');
        await page.fill('input[type="password"]', 'password123');
        await page.selectOption('select', 'User');

        await page.click('button[type="submit"]');

        await expect(page).toHaveURL('/admin/users');
        await expect(page.locator('text=newuser@example.com')).toBeVisible();
    });
});
```

---

## Performance

### Code Splitting

SvelteKit automatically code-splits routes. For large components, use dynamic imports:

```svelte
<script lang="ts">
import { onMount } from 'svelte';

let HeavyComponent: any;

onMount(async () => {
    const module = await import('./HeavyComponent.svelte');
    HeavyComponent = module.default;
});
</script>

{#if HeavyComponent}
    <svelte:component this={HeavyComponent} />
{/if}
```

### Lazy Loading

**Images:**
```svelte
<img
    src={user.avatar}
    alt={user.name}
    loading="lazy"
    class="w-10 h-10 rounded-full"
/>
```

**Pagination:**
- Load only current page data
- Implement infinite scroll for large lists (future)

### Memoization

Use `$derived` for expensive computations:

```svelte
<script lang="ts">
let users = $state<User[]>([]);

// Only recalculates when users changes
let activeUsersCount = $derived(users.filter(u => u.isActive).length);
let adminUsersCount = $derived(users.filter(u => u.role === 'Admin').length);
</script>
```

---

## Accessibility

### Semantic HTML

```svelte
<!-- Good -->
<button onclick={handleClick}>Click me</button>

<!-- Bad -->
<div onclick={handleClick}>Click me</div>
```

### ARIA Labels

```svelte
<button aria-label="Delete user" onclick={handleDelete}>
    <Trash size={16} />
</button>

<input
    type="search"
    aria-label="Search users"
    placeholder="Search..."
/>
```

### Keyboard Navigation

Ensure all interactive elements are keyboard accessible:

```svelte
<button
    onclick={handleClick}
    onkeydown={(e) => e.key === 'Enter' && handleClick()}
>
    Action
</button>
```

### Focus Management

```svelte
<script lang="ts">
import { tick } from 'svelte';

let inputRef: HTMLInputElement;

async function openModal() {
    // ... open modal
    await tick();
    inputRef?.focus();
}
</script>

<input bind:this={inputRef} type="text" />
```

---

## Future Enhancements

### Internationalization (i18n)

**Recommended: sveltekit-i18n**

```svelte
<script lang="ts">
import { t } from '$lib/i18n';
</script>

<h1>{$t('admin.users.title')}</h1>
<button>{$t('common.save')}</button>
```

### Date Formatting (date-fns)

```svelte
<script lang="ts">
import { format, formatDistanceToNow } from 'date-fns';
import { uk } from 'date-fns/locale'; // Ukrainian locale

let user = $props<{ user: User }>();

let formattedDate = $derived(
    format(new Date(user.createdAt), 'PPP', { locale: uk })
);

let lastLoginRelative = $derived(
    user.lastLoginAt
        ? formatDistanceToNow(new Date(user.lastLoginAt), { addSuffix: true, locale: uk })
        : 'Never'
);
</script>

<p>Created: {formattedDate}</p>
<p>Last login: {lastLoginRelative}</p>
```

### Advanced Form Validation (Zod + Superforms)

See [Form Handling](#form-handling) section above.

### Real-time Updates (Server-Sent Events / WebSockets)

```svelte
<script lang="ts">
import { onMount } from 'svelte';

let users = $state<User[]>([]);

onMount(() => {
    const eventSource = new EventSource('/api/users/stream');

    eventSource.addEventListener('user-created', (event) => {
        const newUser = JSON.parse(event.data);
        users = [...users, newUser];
    });

    return () => eventSource.close();
});
</script>
```

### Optimistic UI Updates

```svelte
<script lang="ts">
async function deleteUser(userId: string) {
    // Optimistically remove from UI
    users = users.filter(u => u.id !== userId);

    try {
        await api.deleteUser(userId);
        toast.success('User deleted');
    } catch (err) {
        // Rollback on error
        await loadUsers();
        toast.error('Failed to delete user');
    }
}
</script>
```

---

## Best Practices

### General

1. **Use TypeScript** - Type everything
2. **Prefer Composition** - Small, reusable components
3. **Keep components focused** - Single responsibility
4. **Use semantic HTML** - Accessibility first
5. **Leverage Svelte 5 Runes** - $state, $props, $derived, $effect
6. **Centralize API calls** - Use lib/api/
7. **Define types separately** - Use lib/types/
8. **Use Tailwind utilities** - Avoid custom CSS
9. **Handle errors gracefully** - Toast notifications
10. **Test components** - Unit + E2E tests

### Performance

1. **Use $derived for computed values** - Auto-memoization
2. **Lazy load heavy components** - Dynamic imports
3. **Implement pagination** - Don't load all data at once
4. **Optimize images** - Use loading="lazy"
5. **Code split routes** - SvelteKit does this automatically

### Accessibility

1. **Use semantic HTML** - button, nav, main, etc.
2. **Add ARIA labels** - For icon buttons
3. **Ensure keyboard navigation** - Tab, Enter, Escape
4. **Test with screen readers** - NVDA, JAWS, VoiceOver
5. **Maintain color contrast** - WCAG AA compliance

### Code Quality

1. **Follow .editorconfig** - Consistent formatting
2. **Write descriptive names** - Variables, functions, types
3. **Add comments for complex logic** - Why, not what
4. **Extract magic numbers** - Use named constants
5. **Keep functions small** - Max 50 lines

---

## Common Patterns Reference

### Fetching Data on Mount

```svelte
<script lang="ts">
import { onMount } from 'svelte';

let data = $state<Data[]>([]);
let isLoading = $state(true);

onMount(async () => {
    try {
        data = await api.getData();
    } finally {
        isLoading = false;
    }
});
</script>
```

### Conditional Rendering

```svelte
{#if isLoading}
    <p>Loading...</p>
{:else if error}
    <p class="error">{error}</p>
{:else if items.length > 0}
    {#each items as item (item.id)}
        <div>{item.name}</div>
    {/each}
{:else}
    <p>No items found</p>
{/if}
```

### Event Handling

```svelte
<button onclick={handleClick}>Click</button>
<button onclick={() => handleClickWithArgs(arg1, arg2)}>Click</button>
<input oninput={(e) => handleInput(e.currentTarget.value)} />
```

### Two-way Binding

```svelte
<script lang="ts">
let value = $state('');
</script>

<input bind:value />
<textarea bind:value />
<select bind:value>
    <option value="1">Option 1</option>
</select>
```

---

## Migration Guide (Svelte 4 → 5)

If you encounter old Svelte 4 code, migrate using these patterns:

| Svelte 4 | Svelte 5 |
|----------|----------|
| `export let prop` | `let { prop } = $props()` |
| `let state = value` (reactive) | `let state = $state(value)` |
| `$: derived = compute(state)` | `let derived = $derived(compute(state))` |
| `$: { sideEffect() }` | `$effect(() => { sideEffect() })` |
| `import { writable } from 'svelte/store'` | Use `$state` in `.svelte.ts` files |
| `$store` syntax | Access state directly |

---

## Questions to Ask

When implementing new features, consider:

1. **Does this need to be a new component?** - Or can we extend existing?
2. **Where should state live?** - Local, parent, or global store?
3. **Should this be client-side or server-side?** - +page.ts vs +page.server.ts
4. **Do we need loading states?** - Always for async operations
5. **How do we handle errors?** - Toast, inline, or error boundary?
6. **Is this accessible?** - Keyboard navigation, screen readers?
7. **Should we add tests?** - Yes, for critical user flows
8. **Do we need TypeScript types?** - Yes, always
9. **Can this be reused?** - Extract to lib/components/
10. **Does this follow our patterns?** - Check this document

---

This document is a living guide. Update it as we add new patterns and libraries!
