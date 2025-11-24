<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import UserForm from '$lib/components/UserForm.svelte';
	import { getUserById, updateUser, updateAccount } from '$lib/api/users';
	import type { UserDetail, UpdateUserRequest, UpdateAccountRequest } from '$lib/types/user';
	import { UserRole } from '$lib/types/user';

	let user: UserDetail | null = $state(null);
	let loading = $state(true);
	let error = $state<string | null>(null);
	let userId = $derived(page.params.id!);

	async function loadUser() {
		try {
			user = await getUserById(userId);
		} catch (e) {
			error = e instanceof Error ? e.message : 'Не вдалося завантажити користувача';
		} finally {
			loading = false;
		}
	}

	async function handleSubmit(data: any) {
		try {
			// Update role
			const updateUserData: UpdateUserRequest = {
				role: data.role
			};
			await updateUser(userId, updateUserData);

			// Update account if needed
			const updateAccountData: UpdateAccountRequest = {};
			if (data.email && data.email !== user?.email) {
				updateAccountData.email = data.email;
			}
			if (data.phone && data.phone !== user?.phone) {
				updateAccountData.phone = data.phone;
			}
			if (data.password) {
				updateAccountData.password = data.password;
			}

			if (Object.keys(updateAccountData).length > 0) {
				await updateAccount(userId, updateAccountData);
			}

			alert('Користувача успішно оновлено!');
			goto('/admin/users');
		} catch (e) {
			alert('Не вдалося оновити користувача: ' + (e instanceof Error ? e.message : 'Невідома помилка'));
		}
	}

	function handleCancel() {
		goto('/admin/users');
	}

	onMount(() => {
		loadUser();
	});
</script>

<div class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
	<h1 class="text-3xl font-bold text-gray-900 mb-8">Редагувати користувача</h1>

	{#if loading}
		<p class="text-gray-600">Завантаження...</p>
	{:else if error}
		<div class="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-4">
			{error}
		</div>
	{:else if user}
		<UserForm
			email={user.email || ''}
			phone={user.phone || ''}
			role={user.role === 'Admin' ? UserRole.Admin : UserRole.User}
			isEdit={true}
			onSubmit={handleSubmit}
			onCancel={handleCancel}
		/>
	{/if}
</div>
