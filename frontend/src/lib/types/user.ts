export enum UserRole {
	User = 0,
	Admin = 1
}

export interface User {
	id: string;
	email: string | null;
	phone: string | null;
	role: string;
	isActive: boolean;
	lastLoginAt: string | null;
	createdAt: string;
}

export interface UserDetail extends User {
	updatedAt: string;
}

export interface UserListResult {
	users: User[];
	totalCount: number;
	page: number;
	pageSize: number;
	totalPages: number;
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

export interface UpdateUserStatusRequest {
	isActive: boolean;
}
