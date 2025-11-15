import type {
	User,
	UserDetail,
	UserListResult,
	CreateUserRequest,
	UpdateUserRequest,
	UpdateAccountRequest,
	UpdateUserStatusRequest
} from '$lib/types/user';
import { env } from '$env/dynamic/public';

const API_BASE = `${env.PUBLIC_API_URL || 'http://localhost:5093'}/api/admin/users`;

export async function getUsers(
	page: number = 1,
	pageSize: number = 20,
	includeDeleted: boolean = false
): Promise<UserListResult> {
	const params = new URLSearchParams({
		page: page.toString(),
		pageSize: pageSize.toString(),
		includeDeleted: includeDeleted.toString()
	});

	const response = await fetch(`${API_BASE}?${params}`);
	if (!response.ok) throw new Error('Failed to fetch users');
	return response.json();
}

export async function getUserById(id: string): Promise<UserDetail> {
	const response = await fetch(`${API_BASE}/${id}`);
	if (!response.ok) throw new Error('Failed to fetch user');
	return response.json();
}

export async function createUser(data: CreateUserRequest): Promise<{ id: string }> {
	const response = await fetch(API_BASE, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	});
	if (!response.ok) throw new Error('Failed to create user');
	return response.json();
}

export async function updateUser(id: string, data: UpdateUserRequest): Promise<void> {
	const response = await fetch(`${API_BASE}/${id}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	});
	if (!response.ok) throw new Error('Failed to update user');
}

export async function updateAccount(id: string, data: UpdateAccountRequest): Promise<void> {
	const response = await fetch(`${API_BASE}/${id}/account`, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	});
	if (!response.ok) throw new Error('Failed to update account');
}

export async function updateUserStatus(id: string, data: UpdateUserStatusRequest): Promise<void> {
	const response = await fetch(`${API_BASE}/${id}/status`, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(data)
	});
	if (!response.ok) throw new Error('Failed to update user status');
}

export async function deleteUser(id: string): Promise<void> {
	const response = await fetch(`${API_BASE}/${id}`, {
		method: 'DELETE'
	});
	if (!response.ok) throw new Error('Failed to delete user');
}
