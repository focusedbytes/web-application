<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import AuthMethodManager from '$lib/components/AuthMethodManager.svelte';
	import {
		getUserById,
		updateUser,
		updateProfile,
		addAuthMethod,
		updateAuthMethod,
		removeAuthMethod
	} from '$lib/api/users';
	import type { UserDetail, UpdateUserRequest, UpdateProfileRequest } from '$lib/types/user';
	import { UserRole, AuthMethodType } from '$lib/types/user';

	let user: UserDetail | null = $state(null);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let userId = $derived(page.params.id!);

	// Form state for user profile
	let formData = $state({
		displayName: '',
		role: UserRole.User
	});

	async function loadUser() {
		try {
			user = await getUserById(userId);
			if (user) {
				formData.displayName = user.displayName || '';
				formData.role = user.role === 'Admin' ? UserRole.Admin : UserRole.User;
			}
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to load user';
		} finally {
			loading = false;
		}
	}

	async function handleUpdateProfile() {
		if (!user) return;

		try {
			// Update role
			const updateUserData: UpdateUserRequest = {
				role: formData.role
			};
			await updateUser(userId, updateUserData);

			// Update display name
			const updateProfileData: UpdateProfileRequest = {
				displayName: formData.displayName || null
			};
			await updateProfile(userId, updateProfileData);

			alert('User profile updated successfully!');
			await loadUser(); // Reload user data
		} catch (e) {
			alert('Failed to update user profile: ' + (e instanceof Error ? e.message : 'Unknown error'));
		}
	}

	async function handleAddAuthMethod(
		identifier: string,
		type: AuthMethodType,
		secret: string | null
	) {
		await addAuthMethod(userId, { identifier, type, secret });
		await loadUser(); // Reload user data to show new auth method
	}

	async function handleUpdateAuthMethod(identifier: string, newSecret: string) {
		await updateAuthMethod(userId, { identifier, newSecret });
		await loadUser(); // Reload user data
	}

	async function handleRemoveAuthMethod(identifier: string) {
		await removeAuthMethod(userId, identifier);
		await loadUser(); // Reload user data
	}

	function handleCancel() {
		goto('/admin/users');
	}

	onMount(() => {
		loadUser();
	});
</script>

<div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
	<div class="flex justify-between items-center mb-8">
		<h1 class="text-3xl font-bold text-gray-900">Edit User</h1>
		<button
			onclick={handleCancel}
			class="bg-gray-500 hover:bg-gray-600 text-white font-semibold py-2 px-6 rounded-lg transition"
		>
			Back to Users
		</button>
	</div>

	{#if loading}
		<p class="text-gray-600">Loading...</p>
	{:else if error}
		<div class="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-4">
			{error}
		</div>
	{:else if user}
		<!-- User Profile Section -->
		<div class="bg-white shadow-md rounded-lg p-6 mb-6">
			<h2 class="text-xl font-bold text-gray-900 mb-4">User Profile</h2>

			<form
				onsubmit={(e) => {
					e.preventDefault();
					handleUpdateProfile();
				}}
			>
				<div class="mb-4">
					<label class="block text-gray-700 text-sm font-bold mb-2"> Username </label>
					<input
						type="text"
						value={user.username}
						disabled
						class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 bg-gray-100 cursor-not-allowed"
					/>
					<p class="text-xs text-gray-600 mt-1">Username cannot be changed</p>
				</div>

				<div class="mb-4">
					<label for="displayName" class="block text-gray-700 text-sm font-bold mb-2">
						Display Name
					</label>
					<input
						type="text"
						id="displayName"
						bind:value={formData.displayName}
						placeholder="Optional display name"
						class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
					/>
				</div>

				<div class="mb-4">
					<label for="role" class="block text-gray-700 text-sm font-bold mb-2"> Role * </label>
					<select
						id="role"
						bind:value={formData.role}
						class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
					>
						<option value={UserRole.User}>User</option>
						<option value={UserRole.Admin}>Administrator</option>
					</select>
				</div>

				<div class="mb-4">
					<label class="block text-gray-700 text-sm font-bold mb-2"> Status </label>
					<span
						class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full {user.isActive
							? 'bg-green-100 text-green-800'
							: 'bg-red-100 text-red-800'}"
					>
						{user.isActive ? 'Active' : 'Inactive'}
					</span>
				</div>

				<div class="mb-4">
					<label class="block text-gray-700 text-sm font-bold mb-2"> Last Login </label>
					<p class="text-gray-700">
						{user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleString('en-US') : 'Never'}
					</p>
				</div>

				<div class="mb-4">
					<label class="block text-gray-700 text-sm font-bold mb-2"> Created At </label>
					<p class="text-gray-700">{new Date(user.createdAt).toLocaleString('en-US')}</p>
				</div>

				<button
					type="submit"
					class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-6 rounded transition"
				>
					Save Profile
				</button>
			</form>
		</div>

		<!-- Authentication Methods Section -->
		<AuthMethodManager
			authMethods={user.authMethods}
			onAdd={handleAddAuthMethod}
			onUpdate={handleUpdateAuthMethod}
			onRemove={handleRemoveAuthMethod}
		/>
	{/if}
</div>
