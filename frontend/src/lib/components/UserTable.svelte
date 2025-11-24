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
		if (!date) return 'Ніколи';
		return new Date(date).toLocaleString('uk-UA');
	}

	function getRoleLabel(role: string): string {
		return role === 'Admin' ? 'Адміністратор' : 'Користувач';
	}
</script>

<div class="overflow-x-auto">
	<table class="min-w-full bg-white shadow-md rounded-lg overflow-hidden">
		<thead class="bg-gray-50">
			<tr>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Email
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Роль
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Статус
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Останній вхід
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Створено
				</th>
				<th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
					Дії
				</th>
			</tr>
		</thead>
		<tbody class="divide-y divide-gray-200">
			{#each users as user}
				<tr class="hover:bg-gray-50 transition">
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
						{user.email || ''}
					</td>
					<td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
						{getRoleLabel(user.role)}
					</td>
					<td class="px-6 py-4 whitespace-nowrap">
						{#if user.isActive}
							<span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
								Активний
							</span>
						{:else}
							<span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800">
								Деактивований
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
							Редагувати
						</button>
						<button
							onclick={() => onToggleStatus(user.id, !user.isActive)}
							class="text-amber-600 hover:text-amber-900 transition"
						>
							{user.isActive ? 'Деактивувати' : 'Активувати'}
						</button>
						<button
							onclick={() => onDelete(user.id)}
							class="text-red-600 hover:text-red-900 transition"
						>
							Видалити
						</button>
					</td>
				</tr>
			{/each}
		</tbody>
	</table>
</div>
