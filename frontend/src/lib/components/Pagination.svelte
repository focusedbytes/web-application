<script lang="ts">
	interface Props {
		currentPage: number;
		totalPages: number;
		onPageChange: (page: number) => void;
	}

	let { currentPage, totalPages, onPageChange }: Props = $props();

	function goToPage(page: number) {
		if (page >= 1 && page <= totalPages) {
			onPageChange(page);
		}
	}
</script>

<div class="flex items-center gap-4 mt-8">
	<button
		onclick={() => goToPage(currentPage - 1)}
		disabled={currentPage === 1}
		class="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition"
	>
		Попередня
	</button>

	<span class="font-medium text-gray-700">
		Сторінка {currentPage} з {totalPages}
	</span>

	<button
		onclick={() => goToPage(currentPage + 1)}
		disabled={currentPage === totalPages}
		class="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition"
	>
		Наступна
	</button>
</div>
