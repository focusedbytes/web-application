<script lang="ts">
	import { UserRole, AuthMethodType, type CreateUserRequest } from '$lib/types/user';

	interface Props {
		username?: string;
		role?: UserRole;
		authType?: AuthMethodType;
		authIdentifier?: string;
		isEdit?: boolean;
		onSubmit: (data: CreateUserRequest) => void;
		onCancel: () => void;
	}

	let {
		username = '',
		role = UserRole.User,
		authType = AuthMethodType.Email,
		authIdentifier = '',
		isEdit = false,
		onSubmit,
		onCancel
	}: Props = $props();

	let formData = $state({
		username,
		role,
		authType,
		authIdentifier,
		authSecret: ''
	});

	// Derived label for auth identifier based on type
	let authIdentifierLabel = $derived(() => {
		switch (formData.authType) {
			case AuthMethodType.Email:
				return 'Email';
			case AuthMethodType.Phone:
				return 'Phone Number';
			case AuthMethodType.Google:
				return 'Google ID';
			case AuthMethodType.Apple:
				return 'Apple ID';
			default:
				return 'Identifier';
		}
	});

	// Derived placeholder for auth identifier
	let authIdentifierPlaceholder = $derived(() => {
		switch (formData.authType) {
			case AuthMethodType.Email:
				return 'user@example.com';
			case AuthMethodType.Phone:
				return '+380501234567';
			case AuthMethodType.Google:
				return 'Google user ID';
			case AuthMethodType.Apple:
				return 'Apple user ID';
			default:
				return '';
		}
	});

	// Whether to show password field (only for Email auth)
	let showPasswordField = $derived(formData.authType === AuthMethodType.Email);

	function handleSubmit(e: Event) {
		e.preventDefault();
		onSubmit({
			username: formData.username,
			role: formData.role,
			authIdentifier: formData.authIdentifier,
			authType: formData.authType,
			authSecret: showPasswordField ? formData.authSecret : null
		});
	}
</script>

<form onsubmit={handleSubmit} class="bg-white shadow-md rounded-lg px-8 pt-6 pb-8">
	<div class="mb-6">
		<label for="username" class="block text-gray-700 text-sm font-bold mb-2">
			Username *
		</label>
		<input
			type="text"
			id="username"
			bind:value={formData.username}
			placeholder="username (min 6 characters)"
			required
			minlength="6"
			maxlength="50"
			pattern="^[a-zA-Z0-9_-]+$"
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		/>
		<p class="text-xs text-gray-600 mt-1">
			Only letters, numbers, underscores, and hyphens allowed
		</p>
	</div>

	<div class="mb-6">
		<label for="authType" class="block text-gray-700 text-sm font-bold mb-2">
			Authentication Type *
		</label>
		<select
			id="authType"
			bind:value={formData.authType}
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		>
			<option value={AuthMethodType.Email}>Email</option>
			<option value={AuthMethodType.Phone}>Phone</option>
			<option value={AuthMethodType.Google}>Google</option>
			<option value={AuthMethodType.Apple}>Apple</option>
		</select>
	</div>

	<div class="mb-6">
		<label for="authIdentifier" class="block text-gray-700 text-sm font-bold mb-2">
			{authIdentifierLabel()} *
		</label>
		<input
			type="text"
			id="authIdentifier"
			bind:value={formData.authIdentifier}
			placeholder={authIdentifierPlaceholder()}
			required
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		/>
	</div>

	{#if showPasswordField}
		<div class="mb-6">
			<label for="password" class="block text-gray-700 text-sm font-bold mb-2">
				Password *
			</label>
			<input
				type="password"
				id="password"
				bind:value={formData.authSecret}
				placeholder="Minimum 6 characters"
				required
				minlength="6"
				class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
			/>
		</div>
	{/if}

	<div class="mb-6">
		<label for="role" class="block text-gray-700 text-sm font-bold mb-2"> Role * </label>
		<select
			id="role"
			bind:value={formData.role}
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		>
			<option value={UserRole.User}>User</option>
			<option value={UserRole.Admin}>Administrator</option>
		</select>
	</div>

	<div class="flex items-center gap-4">
		<button
			type="submit"
			class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-6 rounded focus:outline-none focus:shadow-outline transition"
		>
			{isEdit ? 'Update' : 'Create'}
		</button>
		<button
			type="button"
			onclick={onCancel}
			class="bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-6 rounded focus:outline-none focus:shadow-outline transition"
		>
			Cancel
		</button>
	</div>
</form>
