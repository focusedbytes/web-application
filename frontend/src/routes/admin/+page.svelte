<script lang="ts">
	import { goto } from '$app/navigation';
</script>

<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
	<h1 class="text-3xl font-bold text-gray-900 mb-8">Адміністративна панель FocusedBytes</h1>

	<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
		<button
			onclick={() => goto('/admin/users')}
			class="bg-white p-8 rounded-lg shadow-md hover:shadow-xl transition-shadow cursor-pointer text-left"
		>
			<div class="text-5xl mb-4">👥</div>
			<h2 class="text-2xl font-semibold text-gray-800 mb-2">Управління користувачами</h2>
			<p class="text-gray-600">Додавання, редагування та видалення користувачів системи</p>
		</button>

		<div class="bg-white p-8 rounded-lg shadow-md opacity-50 cursor-not-allowed">
			<div class="text-5xl mb-4">📚</div>
			<h2 class="text-2xl font-semibold text-gray-800 mb-2">Курси</h2>
			<p class="text-gray-600">Скоро буде доступно</p>
		</div>

		<div class="bg-white p-8 rounded-lg shadow-md opacity-50 cursor-not-allowed">
			<div class="text-5xl mb-4">📊</div>
			<h2 class="text-2xl font-semibold text-gray-800 mb-2">Статистика</h2>
			<p class="text-gray-600">Скоро буде доступно</p>
		</div>
	</div>
</div>
