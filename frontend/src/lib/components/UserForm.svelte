<script lang="ts">
	import { UserRole } from '$lib/types/user';

	interface FormData {
		email: string;
		phone: string;
		password: string;
		role: UserRole;
	}

	interface Props {
		email?: string;
		phone?: string;
		password?: string;
		role?: UserRole;
		isEdit?: boolean;
		onSubmit: (data: FormData) => void;
		onCancel: () => void;
	}

	let { email = '', phone = '', password = '', role = UserRole.User, isEdit = false, onSubmit, onCancel }: Props = $props();

	let formData = $state({
		email,
		phone,
		password,
		role
	});

	function handleSubmit(e: Event) {
		e.preventDefault();
		onSubmit(formData as any);
	}
</script>

<form onsubmit={handleSubmit} class="bg-white shadow-md rounded-lg px-8 pt-6 pb-8">
	<div class="mb-6">
		<label for="email" class="block text-gray-700 text-sm font-bold mb-2">
			Email (опціонально)
		</label>
		<input
			type="email"
			id="email"
			bind:value={formData.email}
			placeholder="user@example.com"
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		/>
	</div>

	<div class="mb-6">
		<label for="phone" class="block text-gray-700 text-sm font-bold mb-2">
			Телефон (опціонально)
		</label>
		<input
			type="tel"
			id="phone"
			bind:value={formData.phone}
			placeholder="+380501234567"
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		/>
	</div>

	{#if !isEdit || formData.password}
		<div class="mb-6">
			<label for="password" class="block text-gray-700 text-sm font-bold mb-2">
				Пароль {isEdit ? '(залиште порожнім, щоб не змінювати)' : ''}
			</label>
			<input
				type="password"
				id="password"
				bind:value={formData.password}
				placeholder="Мінімум 6 символів"
				required={!isEdit}
				class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
			/>
		</div>
	{/if}

	<div class="mb-6">
		<label for="role" class="block text-gray-700 text-sm font-bold mb-2">
			Роль
		</label>
		<select
			id="role"
			bind:value={formData.role}
			class="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
		>
			<option value={UserRole.User}>Користувач</option>
			<option value={UserRole.Admin}>Адміністратор</option>
		</select>
	</div>

	<div class="flex items-center gap-4">
		<button
			type="submit"
			class="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-6 rounded focus:outline-none focus:shadow-outline transition"
		>
			{isEdit ? 'Оновити' : 'Створити'}
		</button>
		<button
			type="button"
			onclick={onCancel}
			class="bg-gray-500 hover:bg-gray-600 text-white font-bold py-2 px-6 rounded focus:outline-none focus:shadow-outline transition"
		>
			Скасувати
		</button>
	</div>
</form>
