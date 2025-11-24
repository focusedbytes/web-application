<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import UserTable from '$lib/components/UserTable.svelte';
	import Pagination from '$lib/components/Pagination.svelte';
	import { getUsers, deleteUser, updateUserStatus } from '$lib/api/users';
	import type { UserListResult } from '$lib/types/user';

	let users: UserListResult | null = $state(null);
	let currentPage = $state(1);
	let loading = $state(false);
	let error = $state<string | null>(null);

	async function loadUsers(page: number = 1) {
		loading = true;
		error = null;
		try {
			users = await getUsers(page, 20);
			currentPage = page;
		} catch (e) {
			error = e instanceof Error ? e.message : 'Не вдалося завантажити користувачів';
		} finally {
			loading = false;
		}
	}

	async function handleDelete(id: string) {
		if (!confirm('Ви впевнені, що хочете видалити цього користувача?')) return;

		try {
			await deleteUser(id);
			await loadUsers(currentPage);
		} catch (e) {
			alert('Не вдалося видалити користувача');
		}
	}

	async function handleToggleStatus(id: string, isActive: boolean) {
		try {
			await updateUserStatus(id, { isActive });
			await loadUsers(currentPage);
		} catch (e) {
			alert('Не вдалося змінити статус користувача');
		}
	}

	function handleEdit(id: string) {
		goto(`/admin/users/${id}/edit`);
	}

	function handlePageChange(page: number) {
		loadUsers(page);
	}

	onMount(() => {
		loadUsers();
	});
</script>

<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
	<div class="flex justify-between items-center mb-8">
		<h1 class="text-3xl font-bold text-gray-900">Управління користувачами</h1>
		<button
			onclick={() => goto('/admin/users/create')}
			class="bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-6 rounded-lg transition"
		>
			+ Створити користувача
		</button>
	</div>

	{#if loading}
		<p class="text-gray-600">Завантаження...</p>
	{:else if error}
		<div class="bg-red-50 border border-red-200 text-red-800 px-4 py-3 rounded mb-4">
			{error}
		</div>
	{:else if users && users.users.length > 0}
		<UserTable
			users={users.users}
			onEdit={handleEdit}
			onDelete={handleDelete}
			onToggleStatus={handleToggleStatus}
		/>

		<Pagination
			currentPage={users.page}
			totalPages={users.totalPages}
			onPageChange={handlePageChange}
		/>
	{:else}
		<p class="text-gray-600">Користувачів не знайдено</p>
	{/if}
</div>
