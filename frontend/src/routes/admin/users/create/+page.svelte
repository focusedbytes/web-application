<script lang="ts">
	import { goto } from '$app/navigation';
	import UserForm from '$lib/components/UserForm.svelte';
	import { createUser } from '$lib/api/users';
	import type { CreateUserRequest } from '$lib/types/user';

	async function handleSubmit(data: CreateUserRequest) {
		try {
			await createUser(data);
			alert('User created successfully!');
			goto('/admin/users');
		} catch (e) {
			alert('Failed to create user: ' + (e instanceof Error ? e.message : 'Unknown error'));
		}
	}

	function handleCancel() {
		goto('/admin/users');
	}
</script>

<div class="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
	<h1 class="text-3xl font-bold text-gray-900 mb-8">Create New User</h1>

	<UserForm onSubmit={handleSubmit} onCancel={handleCancel} />
</div>
