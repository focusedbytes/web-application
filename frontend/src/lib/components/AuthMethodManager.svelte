<script lang="ts">
	import { AuthMethodType, type AuthMethod } from '$lib/types/user';

	interface Props {
		authMethods: AuthMethod[];
		onAdd: (identifier: string, type: AuthMethodType, secret: string | null) => Promise<void>;
		onUpdate: (identifier: string, newSecret: string) => Promise<void>;
		onRemove: (identifier: string) => Promise<void>;
	}

	let { authMethods, onAdd, onUpdate, onRemove }: Props = $props();

	let showAddForm = $state(false);
	let showUpdateForm = $state<string | null>(null);
	let loading = $state(false);

	// Add form state
	let addFormData = $state({
		identifier: '',
		type: AuthMethodType.Email,
		secret: ''
	});

	// Update form state
	let updateFormData = $state({
		newSecret: ''
	});

	function formatAuthMethodType(type: string): string {
		return type;
	}

	function formatDate(date: string): string {
		return new Date(date).toLocaleString('en-US');
	}

	function shouldShowSecretField(type: AuthMethodType): boolean {
		return type === AuthMethodType.Email;
	}

	async function handleAdd() {
		loading = true;
		try {
			const secret = shouldShowSecretField(addFormData.type) ? addFormData.secret : null;
			await onAdd(addFormData.identifier, addFormData.type, secret);
			// Reset form
			addFormData = {
				identifier: '',
				type: AuthMethodType.Email,
				secret: ''
			};
			showAddForm = false;
		} catch (e) {
			alert('Failed to add authentication method: ' + (e instanceof Error ? e.message : 'Unknown error'));
		} finally {
			loading = false;
		}
	}

	async function handleUpdate(identifier: string) {
		loading = true;
		try {
			await onUpdate(identifier, updateFormData.newSecret);
			updateFormData.newSecret = '';
			showUpdateForm = null;
		} catch (e) {
			alert('Failed to update authentication method: ' + (e instanceof Error ? e.message : 'Unknown error'));
		} finally {
			loading = false;
		}
	}

	async function handleRemove(identifier: string) {
		if (authMethods.length <= 1) {
			alert('Cannot remove the last authentication method. Users must have at least one auth method.');
			return;
		}

		if (!confirm(`Are you sure you want to remove this authentication method?\n${identifier}`)) {
			return;
		}

		loading = true;
		try {
			await onRemove(identifier);
		} catch (e) {
			alert('Failed to remove authentication method: ' + (e instanceof Error ? e.message : 'Unknown error'));
		} finally {
			loading = false;
		}
	}

	function toggleAddForm() {
		showAddForm = !showAddForm;
		if (!showAddForm) {
			addFormData = {
				identifier: '',
				type: AuthMethodType.Email,
				secret: ''
			};
		}
	}

	function toggleUpdateForm(identifier: string) {
		if (showUpdateForm === identifier) {
			showUpdateForm = null;
			updateFormData.newSecret = '';
		} else {
			showUpdateForm = identifier;
			updateFormData.newSecret = '';
		}
	}
</script>

<div class="bg-white shadow-md rounded-lg p-6">
	<div class="flex justify-between items-center mb-4">
		<h2 class="text-xl font-bold text-gray-900">Authentication Methods</h2>
		<button
			onclick={toggleAddForm}
			disabled={loading}
			class="bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-4 rounded transition disabled:opacity-50"
		>
			{showAddForm ? 'Cancel' : '+ Add Method'}
		</button>
	</div>

	{#if showAddForm}
		<form
			onsubmit={(e) => {
				e.preventDefault();
				handleAdd();
			}}
			class="bg-gray-50 p-4 rounded-lg mb-4"
		>
			<h3 class="text-lg font-semibold mb-3">Add New Authentication Method</h3>

			<div class="mb-4">
				<label for="add-type" class="block text-gray-700 text-sm font-bold mb-2">
					Type *
				</label>
				<select
					id="add-type"
					bind:value={addFormData.type}
					class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
				>
					<option value={AuthMethodType.Email}>Email</option>
					<option value={AuthMethodType.Phone}>Phone</option>
					<option value={AuthMethodType.Google}>Google</option>
					<option value={AuthMethodType.Apple}>Apple</option>
				</select>
			</div>

			<div class="mb-4">
				<label for="add-identifier" class="block text-gray-700 text-sm font-bold mb-2">
					Identifier *
				</label>
				<input
					type="text"
					id="add-identifier"
					bind:value={addFormData.identifier}
					placeholder={addFormData.type === AuthMethodType.Email ? 'user@example.com' : 'Identifier'}
					required
					class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
				/>
			</div>

			{#if shouldShowSecretField(addFormData.type)}
				<div class="mb-4">
					<label for="add-secret" class="block text-gray-700 text-sm font-bold mb-2">
						Password *
					</label>
					<input
						type="password"
						id="add-secret"
						bind:value={addFormData.secret}
						placeholder="Minimum 6 characters"
						required
						minlength="6"
						class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
					/>
				</div>
			{/if}

			<button
				type="submit"
				disabled={loading}
				class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded transition disabled:opacity-50"
			>
				Add
			</button>
		</form>
	{/if}

	<div class="space-y-3">
		{#each authMethods as authMethod}
			<div class="border rounded-lg p-4 bg-gray-50">
				<div class="flex justify-between items-start">
					<div class="flex-1">
						<div class="flex items-center gap-3 mb-2">
							<span class="px-3 py-1 bg-indigo-100 text-indigo-800 text-sm font-semibold rounded">
								{formatAuthMethodType(authMethod.type)}
							</span>
							<span class="text-gray-900 font-medium">{authMethod.identifier}</span>
						</div>
						<p class="text-sm text-gray-600">Added: {formatDate(authMethod.createdAt)}</p>

						{#if showUpdateForm === authMethod.identifier}
							<form
								onsubmit={(e) => {
									e.preventDefault();
									handleUpdate(authMethod.identifier);
								}}
								class="mt-3 p-3 bg-white rounded border"
							>
								<label
									for="update-secret-{authMethod.identifier}"
									class="block text-gray-700 text-sm font-bold mb-2"
								>
									New Password *
								</label>
								<div class="flex gap-2">
									<input
										type="password"
										id="update-secret-{authMethod.identifier}"
										bind:value={updateFormData.newSecret}
										placeholder="Minimum 6 characters"
										required
										minlength="6"
										class="flex-1 shadow appearance-none border rounded py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500"
									/>
									<button
										type="submit"
										disabled={loading}
										class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded transition disabled:opacity-50"
									>
										Save
									</button>
								</div>
							</form>
						{/if}
					</div>

					<div class="flex gap-2">
						{#if authMethod.type === 'Email'}
							<button
								onclick={() => toggleUpdateForm(authMethod.identifier)}
								disabled={loading}
								class="text-blue-600 hover:text-blue-900 text-sm font-medium transition disabled:opacity-50"
							>
								{showUpdateForm === authMethod.identifier ? 'Cancel' : 'Update'}
							</button>
						{/if}
						<button
							onclick={() => handleRemove(authMethod.identifier)}
							disabled={loading || authMethods.length <= 1}
							class="text-red-600 hover:text-red-900 text-sm font-medium transition disabled:opacity-50"
							title={authMethods.length <= 1 ? 'Cannot remove the last auth method' : 'Remove this auth method'}
						>
							Remove
						</button>
					</div>
				</div>
			</div>
		{/each}
	</div>
</div>
