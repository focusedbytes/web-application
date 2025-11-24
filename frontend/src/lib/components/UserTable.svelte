<script lang="ts">
	import type { User } from '$lib/types/user';

	interface Props {
		users: User[];
		onEdit: (id: string) => void;
		onDelete: (id: string) => void;
		onToggleStatus: (id: string, isActive: boolean) => void;
	}

	let { users, onEdit, onDelete, onToggleStatus }: Props = $props();

	function formatDate(date: string | null): string {
		if (!date) return 'Never';
		return new Date(date).toLocaleString('en-US');
	}

	function getRoleLabel(role: string): string {
		return role === 'Admin' ? 'Administrator' : 'User';
	}

	function formatAuthMethods(authMethods: User['authMethods']): string {
		if (!authMethods || authMethods.length === 0) return 'None';
		return authMethods.map((am) => `${am.type}: ${am.identifier}`).join(', ');
	}

	function getPrimaryAuthMethod(authMethods: User['authMethods']): string {
		if (!authMethods || authMethods.length === 0) return 'None';
		// Return the first auth method or prioritize email
		const emailMethod = authMethods.find((am) => am.type === 'Email');
		const primaryMethod = emailMethod || authMethods[0];
		return `${primaryMethod.type}: ${primaryMethod.identifier}`;
	}
</script>

<div class="overflow-x-auto">
	<table class="min-w-full bg-white shadow-md rounded-lg overflow-hidden">
		<thead class="bg-gray-50">
			<tr>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Username
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Display Name
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Primary Auth
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Role
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Status
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Last Login
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Created
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Actions
				</th>
			</tr>
		</thead>
		<tbody class="divide-y divide-gray-200">
			{#each users as user}
				<tr class="hover:bg-gray-50 transition">
					<td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
						{user.username}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
						{user.displayName || '-'}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
						{getPrimaryAuthMethod(user.authMethods)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
						{getRoleLabel(user.role)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						{#if user.isActive}
							<span
								class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800"
							>
								Active
							</span>
						{:else}
							<span
								class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800"
							>
								Inactive
							</span>
						{/if}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
						{formatDate(user.lastLoginAt)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
						{formatDate(user.createdAt)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
						<button
							onclick={() => onEdit(user.id)}
							class="text-blue-600 hover:text-blue-900 transition"
						>
							Edit
						</button>
						<button
							onclick={() => onToggleStatus(user.id, !user.isActive)}
							class="text-amber-600 hover:text-amber-900 transition"
						>
							{user.isActive ? 'Deactivate' : 'Activate'}
						</button>
						<button
							onclick={() => onDelete(user.id)}
							class="text-red-600 hover:text-red-900 transition"
						>
							Delete
						</button>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
