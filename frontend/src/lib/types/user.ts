export enum UserRole {
	User = 0,
	Admin = 1
}

export enum AuthMethodType {
	Email = 0,
	Phone = 1,
	Google = 2,
	Apple = 3
}

export interface AuthMethod {
	identifier: string;
	type: string;
	createdAt: string;
}

export interface User {
	id: string;
	username: string;
	displayName: string | null;
	role: string;
	isActive: boolean;
	lastLoginAt: string | null;
	createdAt: string;
	authMethods: AuthMethod[];
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
	username: string;
	role: UserRole;
	authIdentifier: string;
	authType: AuthMethodType;
	authSecret: string | null;
}

export interface UpdateUserRequest {
	role: UserRole;
}

export interface UpdateProfileRequest {
	displayName: string | null;
}

export interface AddAuthMethodRequest {
	identifier: string;
	type: AuthMethodType;
	secret: string | null;
}

export interface UpdateAuthMethodRequest {
	identifier: string;
	newSecret: string | null;
}

export interface UpdateUserStatusRequest {
	isActive: boolean;
}
